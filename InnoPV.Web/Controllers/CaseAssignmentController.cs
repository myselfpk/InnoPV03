using InnoPV.Domain.Entities;
using InnoPV.Domain.Identity;
using InnoPV.Infrastructure.Data;
using InnoPV.Web.Models.CaseAssignment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InnoPV.Web.Services.Email;

namespace InnoPV.Web.Controllers;

[Authorize(Roles = AppRoles.Admin + "," + AppRoles.PvManager)]
public class CaseAssignmentController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAppEmailSender _emailSender;

    public CaseAssignmentController(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    IAppEmailSender emailSender)
    {
        _context = context;
        _userManager = userManager;
        _emailSender = emailSender;
    }

    // ============================================================
    // ASSIGNMENT LIST
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var query = _context.PvCases
            .Where(x => !x.IsDeleted);

        if (!User.IsInRole(AppRoles.Admin))
        {
            query = query.Where(x =>
                x.CurrentAssignedRole == AppRoles.PvManager ||
                x.CurrentAssignedRole == AppRoles.MedicalReviewer ||
                x.Status == InnoPV.Domain.Enums.PvCaseStatus.SubmittedToPvManager ||
                x.Status == InnoPV.Domain.Enums.PvCaseStatus.ResubmittedToPvManager ||
                x.Status == InnoPV.Domain.Enums.PvCaseStatus.ReturnedByMedicalReviewer ||
                x.Status == InnoPV.Domain.Enums.PvCaseStatus.ForwardedToMedicalReviewer);
        }

        var cases = await query
            .OrderByDescending(x => x.CreatedOnUtc)
            .Select(x => new CaseAssignmentListItemViewModel
            {
                Id = x.Id,
                CaseNo = x.CaseNo,
                CaseSource = x.CaseSource,
                ReceiptDate = x.ReceiptDate,
                InitialReporterName = x.InitialReporterName,
                InitialPatientIdentifier = x.InitialPatientIdentifier,
                InitialProductName = x.InitialProductName,
                InitialEventTerm = x.InitialEventTerm,
                IsSerious = x.IsSerious,
                DueDate = x.DueDate,
                Status = x.Status,
                CurrentAssignedRole = x.CurrentAssignedRole,
                CurrentAssignedUserId = x.CurrentAssignedUserId,
                CreatedOnUtc = x.CreatedOnUtc
            })
            .ToListAsync();

        var assignedUserIds = cases
            .Where(x => !string.IsNullOrWhiteSpace(x.CurrentAssignedUserId))
            .Select(x => x.CurrentAssignedUserId!)
            .Distinct()
            .ToList();

        var users = await _context.Users
            .Where(x => assignedUserIds.Contains(x.Id))
            .Select(x => new
            {
                x.Id,
                x.UserName,
                x.Email
            })
            .ToListAsync();

        foreach (var item in cases)
        {
            if (!string.IsNullOrWhiteSpace(item.CurrentAssignedUserId))
            {
                var user = users.FirstOrDefault(x => x.Id == item.CurrentAssignedUserId);

                item.CurrentAssignedUserName = user?.Email ?? user?.UserName;
            }
        }

        return View(cases);
    }

    // ============================================================
    // ASSIGN CASE GET
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Assign(long caseId)
    {
        var pvCase = await _context.PvCases
            .FirstOrDefaultAsync(x => x.Id == caseId && !x.IsDeleted);

        if (pvCase == null)
        {
            TempData["ErrorMessage"] = "Case not found.";
            return RedirectToAction(nameof(Index));
        }

        var assignedUserName = string.Empty;

        if (!string.IsNullOrWhiteSpace(pvCase.CurrentAssignedUserId))
        {
            var assignedUser = await _userManager.FindByIdAsync(pvCase.CurrentAssignedUserId);

            assignedUserName = assignedUser?.Email ?? assignedUser?.UserName ?? string.Empty;
        }

        var model = new CaseAssignmentViewModel
        {
            PvCaseId = pvCase.Id,
            CaseNo = pvCase.CaseNo,
            Status = pvCase.Status,
            CurrentAssignedRole = pvCase.CurrentAssignedRole,
            CurrentAssignedUserId = pvCase.CurrentAssignedUserId,
            CurrentAssignedUserName = assignedUserName,
            AssignToRole = pvCase.CurrentAssignedRole ?? string.Empty,
            AssignToUserId = pvCase.CurrentAssignedUserId ?? string.Empty
        };

        await LoadRoleOptionsAsync(model);
        await LoadUserOptionsAsync(model, model.AssignToRole);

        return View(model);
    }

    // ============================================================
    // ASSIGN CASE POST
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(CaseAssignmentViewModel model)
    {
        await LoadRoleOptionsAsync(model);
        await LoadUserOptionsAsync(model, model.AssignToRole);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (!IsRoleAllowedForAssignment(model.AssignToRole))
        {
            ModelState.AddModelError(nameof(model.AssignToRole), "You are not allowed to assign case to this role.");
            return View(model);
        }

        var pvCase = await _context.PvCases
            .FirstOrDefaultAsync(x => x.Id == model.PvCaseId && !x.IsDeleted);

        if (pvCase == null)
        {
            TempData["ErrorMessage"] = "Case not found.";
            return RedirectToAction(nameof(Index));
        }

        var assignedUser = await _userManager.FindByIdAsync(model.AssignToUserId);

        if (assignedUser == null)
        {
            ModelState.AddModelError(nameof(model.AssignToUserId), "Selected user not found.");
            return View(model);
        }

        var userHasRole = await _userManager.IsInRoleAsync(assignedUser, model.AssignToRole);

        if (!userHasRole)
        {
            ModelState.AddModelError(nameof(model.AssignToUserId), "Selected user does not belong to selected role.");
            return View(model);
        }

        var currentUserId = _userManager.GetUserId(User);
        var currentUserName = User.Identity?.Name;

        var oldRole = pvCase.CurrentAssignedRole;
        var oldUserId = pvCase.CurrentAssignedUserId;
        var oldUserName = string.Empty;

        if (!string.IsNullOrWhiteSpace(oldUserId))
        {
            var oldUser = await _userManager.FindByIdAsync(oldUserId);
            oldUserName = oldUser?.Email ?? oldUser?.UserName ?? string.Empty;
        }

        var newUserName = assignedUser.Email ?? assignedUser.UserName ?? string.Empty;

        pvCase.CurrentAssignedRole = model.AssignToRole;
        pvCase.CurrentAssignedUserId = model.AssignToUserId;
        pvCase.ModifiedBy = currentUserId;
        pvCase.ModifiedOnUtc = DateTime.UtcNow;

        AddAuditTrail(
            pvCase.Id,
            pvCase.CaseNo,
            "PvCase",
            pvCase.Id,
            "Case Assignment",
            "CurrentAssignedRole",
            oldRole,
            model.AssignToRole,
            model.Remarks,
            currentUserId,
            currentUserName);

        AddAuditTrail(
            pvCase.Id,
            pvCase.CaseNo,
            "PvCase",
            pvCase.Id,
            "Case Assignment",
            "CurrentAssignedUser",
            oldUserName,
            newUserName,
            model.Remarks,
            currentUserId,
            currentUserName);

        AddCaseComment(
            pvCase.Id,
            "Assignment",
            model.AssignToRole,
            "Assignment",
            $"Case assigned to {newUserName}. {model.Remarks}",
            currentUserId);

        await _context.SaveChangesAsync();
        if (!string.IsNullOrWhiteSpace(assignedUser.Email))
        {
            var subject = $"InnoPV Case Assigned: {pvCase.CaseNo}";

            var body = PvEmailTemplateHelper.BuildCaseNotificationBody(
                "Case Assigned",
                pvCase.CaseNo,
                pvCase.InitialProductName,
                pvCase.InitialEventTerm,
                pvCase.Status,
                pvCase.CurrentAssignedRole,
                $"A PV case has been assigned to you by {currentUserName}.",
                model.Remarks);

            await _emailSender.SendEmailAsync(
                assignedUser.Email,
                subject,
                body);
        }

        TempData["SuccessMessage"] = $"Case {pvCase.CaseNo} assigned successfully.";
        return RedirectToAction(nameof(Index));
    }

    // ============================================================
    // AJAX: GET USERS BY ROLE
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> GetUsersByRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role) || !IsRoleAllowedForAssignment(role))
        {
            return Json(new List<AssignableUserOptionViewModel>());
        }

        var users = await _userManager.GetUsersInRoleAsync(role);

        var result = users
            .OrderBy(x => x.Email ?? x.UserName)
            .Select(x => new AssignableUserOptionViewModel
            {
                UserId = x.Id,
                DisplayName = x.Email ?? x.UserName ?? x.Id
            })
            .ToList();

        return Json(result);
    }

    // ============================================================
    // PRIVATE METHODS
    // ============================================================

    private Task LoadRoleOptionsAsync(CaseAssignmentViewModel model)
    {
        var roles = GetAllowedRolesForAssignment();

        model.RoleOptions = roles
            .Select(x => new SelectListItem
            {
                Value = x,
                Text = x
            })
            .ToList();

        return Task.CompletedTask;
    }

    private async Task LoadUserOptionsAsync(CaseAssignmentViewModel model, string? role)
    {
        model.UserOptions = new List<SelectListItem>();

        if (string.IsNullOrWhiteSpace(role) || !IsRoleAllowedForAssignment(role))
        {
            return;
        }

        var users = await _userManager.GetUsersInRoleAsync(role);

        model.UserOptions = users
            .OrderBy(x => x.Email ?? x.UserName)
            .Select(x => new SelectListItem
            {
                Value = x.Id,
                Text = x.Email ?? x.UserName ?? x.Id
            })
            .ToList();
    }

    private List<string> GetAllowedRolesForAssignment()
    {
        if (User.IsInRole(AppRoles.Admin))
        {
            return new List<string>
            {
                AppRoles.PvAssociate,
                AppRoles.PvManager,
                AppRoles.MedicalReviewer
            };
        }

        return new List<string>
        {
            AppRoles.PvAssociate,
            AppRoles.PvManager,
            AppRoles.MedicalReviewer
        };
    }

    private bool IsRoleAllowedForAssignment(string role)
    {
        return GetAllowedRolesForAssignment().Contains(role);
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
}