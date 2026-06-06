using InnoPV.Domain.Entities;
using InnoPV.Domain.Enums;
using InnoPV.Domain.Identity;
using InnoPV.Infrastructure.Data;
using InnoPV.Web.Models.Workflow;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InnoPV.Web.Services.Email;
using InnoPV.Web.Services.CaseClosure;
using InnoPV.Web.Services.Workflow;

namespace InnoPV.Web.Controllers;

[Authorize(Roles = AppRoles.Admin + "," + AppRoles.PvAssociate + "," + AppRoles.PvManager + "," + AppRoles.MedicalReviewer)]
public class CaseWorkflowController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAppEmailSender _emailSender;
    private readonly ICaseClosureValidationService _closureValidationService;
    private readonly ICaseWorkflowTransitionService _caseWorkflowTransitionService;

    public CaseWorkflowController(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    IAppEmailSender emailSender,
    ICaseClosureValidationService closureValidationService,
    ICaseWorkflowTransitionService caseWorkflowTransitionService)
    {
        _context = context;
        _userManager = userManager;
        _emailSender = emailSender;
        _closureValidationService = closureValidationService;
        _caseWorkflowTransitionService = caseWorkflowTransitionService;
    }

    // ============================================================
    // CHECKLIST CAPTURE SCREEN
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Checklist(long caseId)
    {
        var pvCase = await _context.PvCases
            .FirstOrDefaultAsync(x => x.Id == caseId && !x.IsDeleted);

        if (pvCase == null)
        {
            TempData["ErrorMessage"] = "Case not found.";
            return RedirectToAction("Index", "PvCase");
        }

        ViewBag.IsReadOnly = pvCase.Status == PvCaseStatus.CaseClosed
                     || pvCase.Status == PvCaseStatus.CaseFinalized
                     || pvCase.Status == PvCaseStatus.Submitted
                     || pvCase.Status == PvCaseStatus.MarkedAsDuplicate
                     || pvCase.Status == PvCaseStatus.MarkedAsInvalid;

        var role = GetCurrentWorkflowRole(pvCase);
        var stage = GetStageByRole(role);

        var checklistMaster = await _context.ChecklistMasters
            .Where(x =>
                x.ApplicableRole == role &&
                x.ApplicableStage == stage &&
                x.IsActive &&
                !x.IsDeleted)
            .OrderByDescending(x => x.EffectiveFrom)
            .FirstOrDefaultAsync();

        if (checklistMaster == null)
        {
            TempData["ErrorMessage"] = $"No active checklist found for role: {role}.";
            return RedirectToAction("Index", "PvCase");
        }

        var checklistItems = await _context.ChecklistItems
            .Where(x =>
                x.ChecklistMasterId == checklistMaster.Id &&
                x.IsActive &&
                !x.IsDeleted)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.ItemText)
            .ToListAsync();

        var itemIds = checklistItems.Select(x => x.Id).ToList();

        var savedResponses = await _context.CaseChecklistResponses
            .Where(x =>
                x.PvCaseId == caseId &&
                itemIds.Contains(x.ChecklistItemId) &&
                !x.IsDeleted)
            .ToListAsync();

        var model = new CaseChecklistCaptureViewModel
        {
            PvCaseId = pvCase.Id,
            CaseNo = pvCase.CaseNo,
            CurrentStatus = pvCase.Status,
            ApplicableRole = role,
            ApplicableStage = stage,
            ChecklistMasterId = checklistMaster.Id,
            ChecklistName = checklistMaster.ChecklistName,
            Items = checklistItems.Select(item =>
            {
                var response = savedResponses.FirstOrDefault(x => x.ChecklistItemId == item.Id);

                return new CaseChecklistItemCaptureViewModel
                {
                    ChecklistItemId = item.Id,
                    ItemText = item.ItemText,
                    IsMandatory = item.IsMandatory,
                    DisplayOrder = item.DisplayOrder,
                    IsChecked = response?.IsChecked ?? false,
                    Remarks = response?.Remarks
                };
            }).ToList()
        };

        return View(model);
    }

    // ============================================================
    // SAVE CHECKLIST AS DRAFT
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveChecklist(CaseChecklistCaptureViewModel model)
    {
        var pvCase = await _context.PvCases
            .FirstOrDefaultAsync(x => x.Id == model.PvCaseId && !x.IsDeleted);

        if (pvCase == null)
        {
            TempData["ErrorMessage"] = "Case not found.";
            return RedirectToAction("Index", "PvCase");
        }

        var currentUserId = _userManager.GetUserId(User);

        foreach (var item in model.Items)
        {
            var response = await _context.CaseChecklistResponses
                .FirstOrDefaultAsync(x =>
                    x.PvCaseId == model.PvCaseId &&
                    x.ChecklistItemId == item.ChecklistItemId &&
                    !x.IsDeleted);

            if (response == null)
            {
                response = new CaseChecklistResponse
                {
                    PvCaseId = model.PvCaseId,
                    ChecklistItemId = item.ChecklistItemId,
                    IsChecked = item.IsChecked,
                    Remarks = item.Remarks?.Trim(),
                    CreatedBy = currentUserId,
                    CreatedOnUtc = DateTime.UtcNow
                };

                _context.CaseChecklistResponses.Add(response);
            }
            else
            {
                response.IsChecked = item.IsChecked;
                response.Remarks = item.Remarks?.Trim();
                response.ModifiedBy = currentUserId;
                response.ModifiedOnUtc = DateTime.UtcNow;
            }
        }

        pvCase.ModifiedBy = currentUserId;
        pvCase.ModifiedOnUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();


        TempData["SuccessMessage"] = "Checklist saved successfully.";
        return RedirectToAction(nameof(Checklist), new { caseId = model.PvCaseId });
    }

    // ============================================================
    // SUBMIT TO NEXT ROLE
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitToNextRole(CaseChecklistCaptureViewModel model)
    {
        var pvCase = await _context.PvCases
            .FirstOrDefaultAsync(x => x.Id == model.PvCaseId && !x.IsDeleted);

        if (pvCase == null)
        {
            TempData["ErrorMessage"] = "Case not found.";
            return RedirectToAction("Index", "PvCase");
        }

        var currentRole = GetCurrentWorkflowRole(pvCase);

        var submitValidation = _caseWorkflowTransitionService.ValidateSubmit(currentRole, pvCase.Status);
        if (!submitValidation.IsAllowed)
        {
            TempData["ErrorMessage"] = submitValidation.Message;
            return RedirectToAction(nameof(Checklist), new { caseId = model.PvCaseId });
        }

        await SaveChecklistResponsesAsync(model);

        var mandatoryMissing = model.Items
            .Where(x => x.IsMandatory && !x.IsChecked)
            .Select(x => x.ItemText)
            .ToList();

        if (mandatoryMissing.Any())
        {
            TempData["ErrorMessage"] = "Please complete all mandatory checklist items before submitting the case.";
            return RedirectToAction(nameof(Checklist), new { caseId = model.PvCaseId });
        }

        var currentUserId = _userManager.GetUserId(User);
        var oldStatus = pvCase.Status.ToString();

        if (currentRole == AppRoles.PvAssociate)
        {
            pvCase.Status = pvCase.Status == PvCaseStatus.ReturnedByPvManager
                ? PvCaseStatus.ResubmittedToPvManager
                : PvCaseStatus.SubmittedToPvManager;

            pvCase.CurrentAssignedRole = AppRoles.PvManager;
            pvCase.CurrentAssignedUserId = null;
        }
        else if (currentRole == AppRoles.PvManager)
        {
            pvCase.Status = PvCaseStatus.ForwardedToMedicalReviewer;
            pvCase.CurrentAssignedRole = AppRoles.MedicalReviewer;
            pvCase.CurrentAssignedUserId = null;
        }
        else if (currentRole == AppRoles.MedicalReviewer)
        {
            var closureValidation = await _closureValidationService
                .ValidateCaseForClosureAsync(pvCase.Id);

            if (!closureValidation.CanClose)
            {
                TempData["ErrorMessage"] = "Case cannot be closed. Please complete closure validation requirements.";

                return RedirectToAction(
                    "Validate",
                    "CaseClosure",
                    new { caseId = pvCase.Id });
            }

            pvCase.Status = PvCaseStatus.CaseClosed;
            pvCase.CurrentAssignedRole = AppRoles.MedicalReviewer;
            pvCase.CurrentAssignedUserId = currentUserId;
        }

        AddAuditTrail(
            pvCase.Id,
            pvCase.CaseNo,
            "PvCase",
            pvCase.Id,
            "Workflow Submit",
            "Status",
            oldStatus,
            pvCase.Status.ToString(),
            model.WorkflowComment,
            currentUserId);

        pvCase.ModifiedBy = currentUserId;
        pvCase.ModifiedOnUtc = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(model.WorkflowComment))
        {
            AddCaseComment(
                pvCase.Id,
                currentRole,
                pvCase.CurrentAssignedRole ?? string.Empty,
                "Submit",
                model.WorkflowComment.Trim(),
                currentUserId);
        }

        await _context.SaveChangesAsync();

        if (currentRole == AppRoles.PvAssociate)
        {
            await SendWorkflowNotificationAsync(
                pvCase,
                "Submitted to PV Manager",
                "A case has been submitted to PV Manager for QC review.",
                model.WorkflowComment);
        }
        else if (currentRole == AppRoles.PvManager)
        {
            await SendWorkflowNotificationAsync(
                pvCase,
                "Forwarded to Medical Reviewer",
                "A case has been forwarded to Medical Reviewer for medical assessment.",
                model.WorkflowComment);
        }
        else if (currentRole == AppRoles.MedicalReviewer)
        {
            await SendWorkflowNotificationAsync(
                pvCase,
                "Case Closed",
                "A case has been closed by Medical Reviewer.",
                model.WorkflowComment);
        }

        TempData["SuccessMessage"] = currentRole == AppRoles.MedicalReviewer
            ? "Case closed successfully."
            : "Case submitted successfully.";

        return RedirectToAction("Index", "PvCase");
    }

    // ============================================================
    // RETURN CASE SCREEN
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Return(long caseId)
    {
        var pvCase = await _context.PvCases
            .FirstOrDefaultAsync(x => x.Id == caseId && !x.IsDeleted);

        if (pvCase == null)
        {
            TempData["ErrorMessage"] = "Case not found.";
            return RedirectToAction("Index", "PvCase");
        }

        var fromRole = GetCurrentWorkflowRole(pvCase);
        var toRole = GetReturnRole(fromRole);

        var returnValidation = _caseWorkflowTransitionService.ValidateReturn(fromRole, pvCase.Status);
        if (!returnValidation.IsAllowed)
        {
            TempData["ErrorMessage"] = returnValidation.Message;
            return RedirectToAction(nameof(Checklist), new { caseId });
        }

        if (string.IsNullOrWhiteSpace(toRole))
        {
            TempData["ErrorMessage"] = "This case cannot be returned from the current stage.";
            return RedirectToAction(nameof(Checklist), new { caseId });
        }

        var model = new ReturnCaseViewModel
        {
            PvCaseId = pvCase.Id,
            CaseNo = pvCase.CaseNo,
            FromRole = fromRole,
            ToRole = toRole
        };

        return View(model);
    }

    // ============================================================
    // RETURN CASE POST
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Return(ReturnCaseViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var pvCase = await _context.PvCases
            .FirstOrDefaultAsync(x => x.Id == model.PvCaseId && !x.IsDeleted);

        if (pvCase == null)
        {
            TempData["ErrorMessage"] = "Case not found.";
            return RedirectToAction("Index", "PvCase");
        }

        var currentUserId = _userManager.GetUserId(User);
        var oldStatus = pvCase.Status.ToString();

        var returnValidation = _caseWorkflowTransitionService.ValidateReturn(model.FromRole, pvCase.Status);
        if (!returnValidation.IsAllowed)
        {
            TempData["ErrorMessage"] = returnValidation.Message;
            return RedirectToAction(nameof(Checklist), new { caseId = model.PvCaseId });
        }

        if (model.FromRole == AppRoles.PvManager)
        {
            pvCase.Status = PvCaseStatus.ReturnedByPvManager;
            pvCase.CurrentAssignedRole = AppRoles.PvAssociate;
            pvCase.CurrentAssignedUserId = pvCase.CreatedBy;
        }
        else if (model.FromRole == AppRoles.MedicalReviewer)
        {
            pvCase.Status = PvCaseStatus.ReturnedByMedicalReviewer;
            pvCase.CurrentAssignedRole = AppRoles.PvManager;
            pvCase.CurrentAssignedUserId = null;
        }
        else
        {
            TempData["ErrorMessage"] = "PV Associate cannot return the case.";
            return RedirectToAction(nameof(Checklist), new { caseId = model.PvCaseId });
        }

        AddAuditTrail(
    pvCase.Id,
    pvCase.CaseNo,
    "PvCase",
    pvCase.Id,
    "Workflow Return",
    "Status",
    oldStatus,
    pvCase.Status.ToString(),
    model.CommentText,
    currentUserId);

        pvCase.ModifiedBy = currentUserId;
        pvCase.ModifiedOnUtc = DateTime.UtcNow;

        AddCaseComment(
            pvCase.Id,
            model.FromRole,
            model.ToRole,
            "Return",
            model.CommentText.Trim(),
            currentUserId);

        await _context.SaveChangesAsync();

        await SendWorkflowNotificationAsync(
            pvCase,
            "Case Returned",
            $"A case has been returned from {model.FromRole} to {model.ToRole}.",
            model.CommentText);

        TempData["SuccessMessage"] = "Case returned successfully.";
        return RedirectToAction("Index", "PvCase");
    }

    // ============================================================
    // PRIVATE METHODS
    // ============================================================

    private async Task SaveChecklistResponsesAsync(CaseChecklistCaptureViewModel model)
    {
        var currentUserId = _userManager.GetUserId(User);

        foreach (var item in model.Items)
        {
            var response = await _context.CaseChecklistResponses
                .FirstOrDefaultAsync(x =>
                    x.PvCaseId == model.PvCaseId &&
                    x.ChecklistItemId == item.ChecklistItemId &&
                    !x.IsDeleted);

            if (response == null)
            {
                response = new CaseChecklistResponse
                {
                    PvCaseId = model.PvCaseId,
                    ChecklistItemId = item.ChecklistItemId,
                    IsChecked = item.IsChecked,
                    Remarks = item.Remarks?.Trim(),
                    CreatedBy = currentUserId,
                    CreatedOnUtc = DateTime.UtcNow
                };

                _context.CaseChecklistResponses.Add(response);
            }
            else
            {
                response.IsChecked = item.IsChecked;
                response.Remarks = item.Remarks?.Trim();
                response.ModifiedBy = currentUserId;
                response.ModifiedOnUtc = DateTime.UtcNow;
            }
        }
    }

    private string GetCurrentWorkflowRole(PvCase pvCase)
    {
        if (User.IsInRole(AppRoles.Admin))
        {
            if (pvCase.Status == PvCaseStatus.SubmittedToPvManager ||
                pvCase.Status == PvCaseStatus.PvManagerReviewPending ||
                pvCase.Status == PvCaseStatus.PvManagerChecklistPending ||
                pvCase.Status == PvCaseStatus.ResubmittedToPvManager ||
                pvCase.Status == PvCaseStatus.ReturnedByMedicalReviewer)
            {
                return AppRoles.PvManager;
            }

            if (pvCase.Status == PvCaseStatus.ForwardedToMedicalReviewer ||
                pvCase.Status == PvCaseStatus.MedicalReviewPending ||
                pvCase.Status == PvCaseStatus.MedicalReviewerChecklistPending ||
                pvCase.Status == PvCaseStatus.MedicallyApproved)
            {
                return AppRoles.MedicalReviewer;
            }

            return AppRoles.PvAssociate;
        }

        if (User.IsInRole(AppRoles.MedicalReviewer))
        {
            return AppRoles.MedicalReviewer;
        }

        if (User.IsInRole(AppRoles.PvManager))
        {
            return AppRoles.PvManager;
        }

        return AppRoles.PvAssociate;
    }

    private static string GetStageByRole(string role)
    {
        if (role == AppRoles.PvAssociate)
        {
            return "PV Associate Data Entry";
        }

        if (role == AppRoles.PvManager)
        {
            return "PV Manager QC Review";
        }

        if (role == AppRoles.MedicalReviewer)
        {
            return "Medical Reviewer Assessment";
        }

        return "PV Associate Data Entry";
    }

    private static string GetReturnRole(string fromRole)
    {
        if (fromRole == AppRoles.PvManager)
        {
            return AppRoles.PvAssociate;
        }

        if (fromRole == AppRoles.MedicalReviewer)
        {
            return AppRoles.PvManager;
        }

        return string.Empty;
    }

    private void AddCaseComment(
        long pvCaseId,
        string fromRole,
        string toRole,
        string commentType,
        string commentText,
        string? currentUserId)
    {
        var comment = new CaseComment
        {
            PvCaseId = pvCaseId,
            FromRole = fromRole,
            ToRole = toRole,
            CommentType = commentType,
            CommentText = commentText,
            CreatedBy = currentUserId,
            CreatedOnUtc = DateTime.UtcNow
        };

        _context.CaseComments.Add(comment);
    }

    private void AddAuditTrail(
    long pvCaseId,
    string caseNo,
    string entityName,
    long? entityId,
    string actionType,
    string? fieldName,
    string? oldValue,
    string? newValue,
    string? remarks,
    string? currentUserId)
    {
        var userName = User?.Identity?.Name;

        var audit = new AuditTrail
        {
            PvCaseId = pvCaseId,
            CaseNo = caseNo,
            EntityName = entityName,
            EntityId = entityId,
            ActionType = actionType,
            FieldName = fieldName,
            OldValue = oldValue,
            NewValue = newValue,
            Remarks = remarks,
            PerformedByUserId = currentUserId,
            PerformedByUserName = userName,
            PerformedOnUtc = DateTime.UtcNow,
            CreatedBy = currentUserId,
            CreatedOnUtc = DateTime.UtcNow
        };

        _context.AuditTrails.Add(audit);
    }
    private async Task SendWorkflowNotificationAsync(
    PvCase pvCase,
    string notificationTitle,
    string notificationMessage,
    string? remarks)
    {
        var recipientEmails = new List<string>();

        if (!string.IsNullOrWhiteSpace(pvCase.CurrentAssignedUserId))
        {
            var assignedUser = await _userManager.FindByIdAsync(pvCase.CurrentAssignedUserId);

            if (!string.IsNullOrWhiteSpace(assignedUser?.Email))
            {
                recipientEmails.Add(assignedUser.Email);
            }
        }
        else if (!string.IsNullOrWhiteSpace(pvCase.CurrentAssignedRole))
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(pvCase.CurrentAssignedRole);

            recipientEmails.AddRange(
                usersInRole
                    .Where(x => !string.IsNullOrWhiteSpace(x.Email))
                    .Select(x => x.Email!));
        }

        if (!recipientEmails.Any())
        {
            return;
        }

        var subject = $"InnoPV {notificationTitle}: {pvCase.CaseNo}";

        var body = PvEmailTemplateHelper.BuildCaseNotificationBody(
            notificationTitle,
            pvCase.CaseNo,
            pvCase.InitialProductName,
            pvCase.InitialEventTerm,
            pvCase.Status,
            pvCase.CurrentAssignedRole,
            notificationMessage,
            remarks);

        await _emailSender.SendEmailsAsync(
            recipientEmails,
            subject,
            body);
    }
}