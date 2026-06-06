using InnoPV.Domain.Entities;
using InnoPV.Domain.Enums;
using InnoPV.Domain.Identity;
using InnoPV.Infrastructure.Data;
using InnoPV.Web.Models.CaseFollowUp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InnoPV.Web.Controllers;

[Authorize(Roles = AppRoles.Admin + "," + AppRoles.PvAssociate + "," + AppRoles.PvManager + "," + AppRoles.MedicalReviewer)]
public class CaseFollowUpController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CaseFollowUpController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(long caseId)
    {
        var pvCase = await _context.PvCases
            .FirstOrDefaultAsync(x => x.Id == caseId && !x.IsDeleted);

        if (pvCase == null)
        {
            TempData["ErrorMessage"] = "Case not found.";
            return RedirectToAction("Index", "CaseInbox");
        }

        var followUps = await _context.CaseFollowUps
            .Where(x => x.PvCaseId == caseId && !x.IsDeleted)
            .OrderByDescending(x => x.ReceiptDate)
            .ThenByDescending(x => x.CreatedOnUtc)
            .Select(x => new CaseFollowUpListItemViewModel
            {
                Id = x.Id,
                PvCaseId = x.PvCaseId,
                FollowUpNo = x.FollowUpNo,
                ReceiptDate = x.ReceiptDate,
                Source = x.Source,
                ReceivedFrom = x.ReceivedFrom,
                Description = x.Description,
                AdditionalInformation = x.AdditionalInformation,
                IsProcessed = x.IsProcessed,
                ProcessedRemarks = x.ProcessedRemarks,
                ProcessedOnUtc = x.ProcessedOnUtc,
                CreatedBy = x.CreatedBy,
                CreatedOnUtc = x.CreatedOnUtc
            })
            .ToListAsync();

        var model = new CaseFollowUpIndexViewModel
        {
            PvCaseId = pvCase.Id,
            CaseNo = pvCase.CaseNo,
            Status = pvCase.Status,
            ProductName = pvCase.InitialProductName,
            EventTerm = pvCase.InitialEventTerm,
            TotalFollowUps = followUps.Count,
            PendingFollowUps = followUps.Count(x => !x.IsProcessed),
            ProcessedFollowUps = followUps.Count(x => x.IsProcessed),
            CanAddFollowUp = CanAddFollowUp(pvCase),
            FollowUps = followUps
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create(long caseId)
    {
        var pvCase = await _context.PvCases
            .FirstOrDefaultAsync(x => x.Id == caseId && !x.IsDeleted);

        if (pvCase == null)
        {
            TempData["ErrorMessage"] = "Case not found.";
            return RedirectToAction("Index", "CaseInbox");
        }

        if (!CanAddFollowUp(pvCase))
        {
            TempData["ErrorMessage"] = "Follow-up cannot be added for duplicate or invalid cases.";
            return RedirectToAction(nameof(Index), new { caseId });
        }

        var model = new CaseFollowUpCreateViewModel
        {
            PvCaseId = pvCase.Id,
            CaseNo = pvCase.CaseNo,
            ReceiptDate = DateTime.Today
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CaseFollowUpCreateViewModel model)
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
            return RedirectToAction("Index", "CaseInbox");
        }

        if (!CanAddFollowUp(pvCase))
        {
            TempData["ErrorMessage"] = "Follow-up cannot be added for duplicate or invalid cases.";
            return RedirectToAction(nameof(Index), new { caseId = model.PvCaseId });
        }

        var currentUserId = _userManager.GetUserId(User);
        var currentUserName = User.Identity?.Name;
        var oldStatus = pvCase.Status.ToString();

        var followUpNo = await GenerateFollowUpNoAsync(pvCase.Id, pvCase.CaseNo);

        var followUp = new CaseFollowUp
        {
            PvCaseId = pvCase.Id,
            FollowUpNo = followUpNo,
            ReceiptDate = model.ReceiptDate.Date,
            Source = model.Source.Trim(),
            ReceivedFrom = model.ReceivedFrom?.Trim(),
            Description = model.Description.Trim(),
            AdditionalInformation = model.AdditionalInformation?.Trim(),
            IsProcessed = false,
            CreatedBy = currentUserId,
            CreatedOnUtc = DateTime.UtcNow
        };

        _context.CaseFollowUps.Add(followUp);

        pvCase.Status = GetStatusAfterFollowUpReceived(pvCase.Status);
        pvCase.CurrentAssignedRole = AppRoles.PvAssociate;

        if (User.IsInRole(AppRoles.PvAssociate))
        {
            pvCase.CurrentAssignedUserId = currentUserId;
        }
        else
        {
            pvCase.CurrentAssignedUserId = null;
        }

        pvCase.ModifiedBy = currentUserId;
        pvCase.ModifiedOnUtc = DateTime.UtcNow;

        AddCaseComment(
            pvCase.Id,
            "Follow-up",
            AppRoles.PvAssociate,
            "Follow-up Received",
            $"Follow-up received: {followUpNo}. Source: {model.Source}. Description: {model.Description}",
            currentUserId);

        AddAuditTrail(
            pvCase.Id,
            pvCase.CaseNo,
            "PvCase",
            pvCase.Id,
            "Follow-up Received",
            "Status",
            oldStatus,
            pvCase.Status.ToString(),
            $"Follow-up No: {followUpNo}. {model.Description}",
            currentUserId,
            currentUserName);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Follow-up {followUpNo} created successfully.";
        return RedirectToAction(nameof(Index), new { caseId = pvCase.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkProcessed(ProcessFollowUpViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Processed remarks are required.";
            return RedirectToAction(nameof(Index), new { caseId = model.PvCaseId });
        }

        var followUp = await _context.CaseFollowUps
            .Include(x => x.PvCase)
            .FirstOrDefaultAsync(x => x.Id == model.FollowUpId && !x.IsDeleted);

        if (followUp == null)
        {
            TempData["ErrorMessage"] = "Follow-up not found.";
            return RedirectToAction("Index", "CaseInbox");
        }

        var currentUserId = _userManager.GetUserId(User);
        var currentUserName = User.Identity?.Name;

        followUp.IsProcessed = true;
        followUp.ProcessedRemarks = model.ProcessedRemarks.Trim();
        followUp.ProcessedByUserId = currentUserId;
        followUp.ProcessedOnUtc = DateTime.UtcNow;
        followUp.ModifiedBy = currentUserId;
        followUp.ModifiedOnUtc = DateTime.UtcNow;

        AddCaseComment(
            followUp.PvCaseId,
            "Follow-up",
            "System",
            "Follow-up Processed",
            $"Follow-up {followUp.FollowUpNo} processed. Remarks: {model.ProcessedRemarks}",
            currentUserId);

        AddAuditTrail(
            followUp.PvCaseId,
            followUp.PvCase.CaseNo,
            "CaseFollowUp",
            followUp.Id,
            "Follow-up Processed",
            "IsProcessed",
            "False",
            "True",
            model.ProcessedRemarks,
            currentUserId,
            currentUserName);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Follow-up {followUp.FollowUpNo} marked as processed.";
        return RedirectToAction(nameof(Index), new { caseId = followUp.PvCaseId });
    }

    private async Task<string> GenerateFollowUpNoAsync(long pvCaseId, string caseNo)
    {
        var count = await _context.CaseFollowUps
            .CountAsync(x => x.PvCaseId == pvCaseId && !x.IsDeleted);

        var nextNo = count + 1;

        return $"{caseNo}-FU{nextNo:00}";
    }

    private static bool CanAddFollowUp(PvCase pvCase)
    {
        return pvCase.Status != PvCaseStatus.MarkedAsDuplicate
               && pvCase.Status != PvCaseStatus.MarkedAsInvalid;
    }

    private static PvCaseStatus GetStatusAfterFollowUpReceived(PvCaseStatus currentStatus)
    {
        if (currentStatus == PvCaseStatus.CaseClosed ||
            currentStatus == PvCaseStatus.CaseFinalized ||
            currentStatus == PvCaseStatus.Submitted ||
            currentStatus == PvCaseStatus.AcknowledgementPending)
        {
            return PvCaseStatus.Reopened;
        }

        if (currentStatus == PvCaseStatus.MarkedAsDuplicate ||
            currentStatus == PvCaseStatus.MarkedAsInvalid)
        {
            return currentStatus;
        }

        return PvCaseStatus.AdditionalInformationRequired;
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
}