using InnoPV.Domain.Entities;
using InnoPV.Domain.Enums;
using InnoPV.Domain.Identity;
using InnoPV.Infrastructure.Data;
using InnoPV.Web.Models.DuplicateCheck;
using InnoPV.Web.Services.DuplicateCheck;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InnoPV.Web.Services.Security;

namespace InnoPV.Web.Controllers;

[Authorize(Policy = AuthorizationPolicies.AdminOrPvAssociateOrPvManager)]
public class DuplicateCheckController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IDuplicateCheckService _duplicateCheckService;
    private readonly ICaseSecurityService _caseSecurityService;

    public DuplicateCheckController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IDuplicateCheckService duplicateCheckService,
        ICaseSecurityService caseSecurityService)
    {
        _context = context;
        _userManager = userManager;
        _duplicateCheckService = duplicateCheckService;
        _caseSecurityService = caseSecurityService;
    }

    [HttpGet]
    public async Task<IActionResult> Check(long caseId)
    {
        if (!EnsurePermission(PermissionActions.DuplicateCheckView, "You are not allowed to access duplicate check."))
        {
            return RedirectToAction("Index", "CaseInbox");
        }

        var pvCase = await GetCaseAsync(caseId);

        if (pvCase == null)
        {
            TempData["ErrorMessage"] = "Case not found.";
            return RedirectToAction("Index", "CaseInbox");
        }

        if (!CanViewCase(pvCase))
        {
            return RedirectToAction("Index", "CaseInbox");
        }

        var model = await _duplicateCheckService.GetDuplicateCheckAsync(caseId);

        if (model == null)
        {
            TempData["ErrorMessage"] = "Case not found.";
            return RedirectToAction("Index", "CaseInbox");
        }

        model.NotDuplicateRemarks = string.Empty;

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsDuplicate(MarkDuplicateViewModel model)
    {
        if (!EnsurePermission(PermissionActions.DuplicateDecision, "You are not allowed to make duplicate decisions."))
        {
            return RedirectToAction("Index", "CaseInbox");
        }

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Remarks are required to mark the case as duplicate.";
            return RedirectToAction(nameof(Check), new { caseId = model.PvCaseId });
        }

        var pvCase = await _context.PvCases
            .FirstOrDefaultAsync(x => x.Id == model.PvCaseId && !x.IsDeleted);

        var duplicateOfCase = await _context.PvCases
            .FirstOrDefaultAsync(x => x.Id == model.DuplicateOfPvCaseId && !x.IsDeleted);

        if (pvCase == null || duplicateOfCase == null)
        {
            TempData["ErrorMessage"] = "Case or duplicate case not found.";
            return RedirectToAction("Index", "CaseInbox");
        }

        if (!CanEditCase(pvCase) || !CanViewCase(duplicateOfCase))
        {
            return RedirectToAction("Index", "CaseInbox");
        }

        var duplicateCandidate = await _duplicateCheckService.GetDuplicateCandidateAsync(
            pvCase.Id,
            duplicateOfCase.Id);

        if (duplicateCandidate == null)
        {
            TempData["ErrorMessage"] = "Selected case is not a valid duplicate candidate.";
            return RedirectToAction(nameof(Check), new { caseId = pvCase.Id });
        }

        var currentUserId = _userManager.GetUserId(User);
        var currentUserName = User.Identity?.Name;
        var oldStatus = pvCase.Status.ToString();

        var assessment = new CaseDuplicateAssessment
        {
            PvCaseId = pvCase.Id,
            PotentialDuplicatePvCaseId = duplicateOfCase.Id,
            MatchingScore = duplicateCandidate.MatchingScore,
            MatchReasons = duplicateCandidate.MatchReasons,
            IsConfirmedDuplicate = true,
            DecisionRemarks = model.Remarks.Trim(),
            AssessedByUserId = currentUserId,
            AssessedOnUtc = DateTime.UtcNow,
            CreatedBy = currentUserId,
            CreatedOnUtc = DateTime.UtcNow
        };

        _context.CaseDuplicateAssessments.Add(assessment);

        pvCase.Status = PvCaseStatus.MarkedAsDuplicate;
        pvCase.CurrentAssignedRole = null;
        pvCase.CurrentAssignedUserId = null;
        pvCase.ModifiedBy = currentUserId;
        pvCase.ModifiedOnUtc = DateTime.UtcNow;

        AddCaseComment(
            pvCase.Id,
            "Duplicate Check",
            "System",
            "Duplicate",
            $"Case marked as duplicate of {duplicateOfCase.CaseNo}. Remarks: {model.Remarks}",
            currentUserId);

        AddAuditTrail(
            pvCase.Id,
            pvCase.CaseNo,
            "PvCase",
            pvCase.Id,
            "Duplicate Decision",
            "Status",
            oldStatus,
            PvCaseStatus.MarkedAsDuplicate.ToString(),
            $"Duplicate of {duplicateOfCase.CaseNo}. {model.Remarks}",
            currentUserId,
            currentUserName);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Case {pvCase.CaseNo} marked as duplicate of {duplicateOfCase.CaseNo}.";
        return RedirectToAction("Index", "CaseInbox");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsNotDuplicate(MarkNotDuplicateViewModel model)
    {
        if (!EnsurePermission(PermissionActions.DuplicateDecision, "You are not allowed to make duplicate decisions."))
        {
            return RedirectToAction("Index", "CaseInbox");
        }

        if (!ModelState.IsValid)
        {
            var checkModel = await _duplicateCheckService.GetDuplicateCheckAsync(model.CaseId);
            if (checkModel == null)
            {
                TempData["ErrorMessage"] = "Case not found.";
                return RedirectToAction("Index", "CaseInbox");
            }

            checkModel.NotDuplicateRemarks = model.Remarks;
            return View("Check", checkModel);
        }

        var pvCase = await _context.PvCases
            .FirstOrDefaultAsync(x => x.Id == model.CaseId && !x.IsDeleted);

        if (pvCase == null)
        {
            TempData["ErrorMessage"] = "Case not found.";
            return RedirectToAction("Index", "CaseInbox");
        }

        if (!CanEditCase(pvCase))
        {
            return RedirectToAction("Index", "CaseInbox");
        }

        var currentUserId = _userManager.GetUserId(User);
        var currentUserName = User.Identity?.Name;
        var oldStatus = pvCase.Status.ToString();

        var assessment = new CaseDuplicateAssessment
        {
            PvCaseId = pvCase.Id,
            PotentialDuplicatePvCaseId = null,
            MatchingScore = 0,
            MatchReasons = "Duplicate check completed. No duplicate confirmed.",
            IsConfirmedDuplicate = false,
            DecisionRemarks = model.Remarks.Trim(),
            AssessedByUserId = currentUserId,
            AssessedOnUtc = DateTime.UtcNow,
            CreatedBy = currentUserId,
            CreatedOnUtc = DateTime.UtcNow
        };

        _context.CaseDuplicateAssessments.Add(assessment);

        if (pvCase.Status == PvCaseStatus.DuplicateCheckPending)
        {
            pvCase.Status = PvCaseStatus.DataEntryInProgress;
        }

        pvCase.ModifiedBy = currentUserId;
        pvCase.ModifiedOnUtc = DateTime.UtcNow;

        AddCaseComment(
            pvCase.Id,
            "Duplicate Check",
            "System",
            "Not Duplicate",
            $"Duplicate check completed. No duplicate confirmed. Remarks: {model.Remarks}",
            currentUserId);

        AddAuditTrail(
            pvCase.Id,
            pvCase.CaseNo,
            "PvCase",
            pvCase.Id,
            "Duplicate Decision",
            "Status",
            oldStatus,
            pvCase.Status.ToString(),
            model.Remarks,
            currentUserId,
            currentUserName);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Duplicate check completed for case {pvCase.CaseNo}.";
        return RedirectToAction("Index", "CaseInbox");
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

    private async Task<PvCase?> GetCaseAsync(long caseId)
    {
        return await _context.PvCases
            .FirstOrDefaultAsync(x => x.Id == caseId && !x.IsDeleted);
    }

    private bool CanViewCase(PvCase pvCase)
    {
        var currentUserId = _userManager.GetUserId(User);

        if (_caseSecurityService.CanViewCase(pvCase, User, currentUserId))
        {
            return true;
        }

        TempData["ErrorMessage"] = "You are not allowed to view this case.";
        return false;
    }

    private bool CanEditCase(PvCase pvCase)
    {
        var currentUserId = _userManager.GetUserId(User);

        if (_caseSecurityService.CanEditCase(pvCase, User, currentUserId))
        {
            return true;
        }

        TempData["ErrorMessage"] = "You are not allowed to update this case.";
        return false;
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
        string? currentUserId,
        string? currentUserName)
    {
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
            PerformedByUserName = currentUserName,
            PerformedOnUtc = DateTime.UtcNow,
            CreatedBy = currentUserId,
            CreatedOnUtc = DateTime.UtcNow
        };

        _context.AuditTrails.Add(audit);
    }

    private bool EnsurePermission(string action, string message)
    {
        if (_caseSecurityService.CanPerformAction(User, action))
        {
            return true;
        }

        TempData["ErrorMessage"] = message;
        return false;
    }
}
