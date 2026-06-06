using InnoPV.Domain.Entities;
using InnoPV.Domain.Enums;
using InnoPV.Domain.Identity;
using InnoPV.Infrastructure.Data;
using InnoPV.Web.Models.PvCase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InnoPV.Web.Services.Email;
using InnoPV.Web.Services.CaseIntake;
using System.Net;
using System.Text.RegularExpressions;


namespace InnoPV.Web.Controllers;

[Authorize(Roles = AppRoles.Admin + "," + AppRoles.PvAssociate)]
public class PvCaseController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAppEmailSender _emailSender;
    private readonly ICaseIntakeValidationService _caseIntakeValidationService;

    public PvCaseController(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    IAppEmailSender emailSender,
    ICaseIntakeValidationService caseIntakeValidationService)
    {
        _context = context;
        _userManager = userManager;
        _emailSender = emailSender;
        _caseIntakeValidationService = caseIntakeValidationService;
    }

    // ============================================================
    // CASE LIST
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var currentUserId = _userManager.GetUserId(User);

        var query = _context.PvCases
            .Where(x => !x.IsDeleted);

        if (!User.IsInRole(AppRoles.Admin))
        {
            query = query.Where(x =>
                x.CreatedBy == currentUserId ||
                x.CurrentAssignedUserId == currentUserId);
        }

        var entities = await query
            .OrderByDescending(x => x.CreatedOnUtc)
            .ToListAsync();

        var cases = entities
            .Select(x => new PvCaseListItemViewModel
            {
                Id = x.Id,
                CaseNo = x.CaseNo,
                CaseSource = x.CaseSource,
                ReceiptDate = x.ReceiptDate,
                InitialReporterName = x.InitialReporterName,
                InitialPatientIdentifier = x.InitialPatientIdentifier,
                InitialProductName = x.InitialProductName,
                InitialEventTerm = x.InitialEventTerm,
                IsValidCase = x.IsValidCase,
                IsSerious = x.IsSerious,
                CompletenessScore = _caseIntakeValidationService.CalculateCompletenessScore(x),
                DueDate = x.DueDate,
                Status = x.Status
            })
            .ToList();

        return View(cases);
    }

    // ============================================================
    // CREATE CASE
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new PvCaseIntakeViewModel
        {
            ReceiptDate = DateTime.Today
        };

        model.CompletenessScore = _caseIntakeValidationService.Validate(model).CompletenessScore;

        await LoadDropdownsAsync(model);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PvCaseIntakeViewModel model)
    {
        await LoadDropdownsAsync(model);
        model.CompletenessScore = _caseIntakeValidationService.Validate(model).CompletenessScore;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var intakeValidationResult = _caseIntakeValidationService.Validate(model);
        model.CompletenessScore = intakeValidationResult.CompletenessScore;

        AddIntakeValidationIssues(intakeValidationResult);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var currentUserId = _userManager.GetUserId(User);

        var isValidCase =
            model.IsPatientIdentifiable &&
            model.IsReporterIdentifiable &&
            model.IsSuspectProductAvailable &&
            model.IsAdverseEventAvailable;

        var caseNo = await GenerateCaseNoAsync();

        var dueDate = CalculateDueDate(model.ReceiptDate, model.IsSerious);

        var entity = new PvCase
        {
            CaseNo = caseNo,
            CaseSource = model.CaseSource.Trim(),
            ReceiptDate = model.ReceiptDate.Date,

            InitialReporterName = model.InitialReporterName?.Trim(),
            InitialPatientIdentifier = model.InitialPatientIdentifier?.Trim(),
            InitialProductName = model.InitialProductName?.Trim(),
            InitialEventTerm = model.InitialEventTerm?.Trim(),

            IsPatientIdentifiable = model.IsPatientIdentifiable,
            IsReporterIdentifiable = model.IsReporterIdentifiable,
            IsSuspectProductAvailable = model.IsSuspectProductAvailable,
            IsAdverseEventAvailable = model.IsAdverseEventAvailable,

            IsValidCase = isValidCase,
            IsSerious = model.IsSerious,
            IsDuplicateSuspected = false,

            Status = isValidCase
                ? PvCaseStatus.DataEntryInProgress
                : PvCaseStatus.InvalidFollowUpRequired,

            CurrentAssignedUserId = currentUserId,
            CurrentAssignedRole = AppRoles.PvAssociate,

            DueDate = dueDate,
            Narrative = ToPlainText(model.Narrative),
            Remarks = model.Remarks?.Trim(),

            CreatedBy = currentUserId,
            CreatedOnUtc = DateTime.UtcNow
        };

        _context.PvCases.Add(entity);
        await _context.SaveChangesAsync();

        var currentUser = await _userManager.GetUserAsync(User);

        if (!string.IsNullOrWhiteSpace(currentUser?.Email))
        {
            var subject = $"InnoPV Case Created: {caseNo}";

            var body = PvEmailTemplateHelper.BuildCaseNotificationBody(
                "Case Created",
                caseNo,
                entity.InitialProductName,
                entity.InitialEventTerm,
                entity.Status,
                entity.CurrentAssignedRole,
                "A new PV case has been created successfully.",
                entity.Remarks);

            await _emailSender.SendEmailAsync(
                currentUser.Email,
                subject,
                body);
        }

        TempData["SuccessMessage"] = $"Case {caseNo} created successfully.";
        return RedirectToAction(nameof(Index));
    }

    // ============================================================
    // DETAILS
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Details(long id)
    {
        var entity = await _context.PvCases
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (entity == null)
        {
            TempData["ErrorMessage"] = "Case not found.";
            return RedirectToAction(nameof(Index));
        }

        var model = new PvCaseListItemViewModel
        {
            Id = entity.Id,
            CaseNo = entity.CaseNo,
            CaseSource = entity.CaseSource,
            ReceiptDate = entity.ReceiptDate,
            InitialReporterName = entity.InitialReporterName,
            InitialPatientIdentifier = entity.InitialPatientIdentifier,
            InitialProductName = entity.InitialProductName,
            InitialEventTerm = entity.InitialEventTerm,
            IsValidCase = entity.IsValidCase,
            IsSerious = entity.IsSerious,
            CompletenessScore = _caseIntakeValidationService.CalculateCompletenessScore(entity),
            DueDate = entity.DueDate,
            Status = entity.Status
        };

        return View(model);
    }

    // ============================================================
    // EDIT CASE
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Edit(long id)
    {
        var entity = await _context.PvCases
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (entity == null)
        {
            TempData["ErrorMessage"] = "Case not found.";
            return RedirectToAction(nameof(Index));
        }

        if (IsCaseReadOnlyForUpdate(entity.Status))
        {
            TempData["ErrorMessage"] = "This case is read-only and cannot be updated.";
            return RedirectToAction(nameof(Details), new { id = entity.Id });
        }

        var currentUserId = _userManager.GetUserId(User);

        if (!User.IsInRole(AppRoles.Admin) &&
            entity.CreatedBy != currentUserId &&
            entity.CurrentAssignedUserId != currentUserId)
        {
            TempData["ErrorMessage"] = "You are not authorized to update this case.";
            return RedirectToAction(nameof(Index));
        }

        var model = new PvCaseEditViewModel
        {
            Id = entity.Id,
            CaseNo = entity.CaseNo,
            Status = entity.Status,

            CaseSource = entity.CaseSource,
            ReceiptDate = entity.ReceiptDate,

            InitialReporterName = entity.InitialReporterName,
            InitialPatientIdentifier = entity.InitialPatientIdentifier,
            InitialProductName = entity.InitialProductName,
            InitialEventTerm = entity.InitialEventTerm,

            IsPatientIdentifiable = entity.IsPatientIdentifiable,
            IsReporterIdentifiable = entity.IsReporterIdentifiable,
            IsSuspectProductAvailable = entity.IsSuspectProductAvailable,
            IsAdverseEventAvailable = entity.IsAdverseEventAvailable,

            IsSerious = entity.IsSerious,
            CompletenessScore = _caseIntakeValidationService.CalculateCompletenessScore(entity),
            DueDate = entity.DueDate,
            Narrative = entity.Narrative,
            Remarks = entity.Remarks
        };

        await LoadDropdownsAsync(model);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PvCaseEditViewModel model)
    {
        await LoadDropdownsAsync(model);
        model.CompletenessScore = _caseIntakeValidationService.Validate(model).CompletenessScore;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var intakeValidationResult = _caseIntakeValidationService.Validate(model);
        model.CompletenessScore = intakeValidationResult.CompletenessScore;

        AddIntakeValidationIssues(intakeValidationResult);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var entity = await _context.PvCases
            .FirstOrDefaultAsync(x => x.Id == model.Id && !x.IsDeleted);

        if (entity == null)
        {
            TempData["ErrorMessage"] = "Case not found.";
            return RedirectToAction(nameof(Index));
        }

        if (IsCaseReadOnlyForUpdate(entity.Status))
        {
            TempData["ErrorMessage"] = "This case is read-only and cannot be updated.";
            return RedirectToAction(nameof(Details), new { id = entity.Id });
        }

        var currentUserId = _userManager.GetUserId(User);
        var currentUserName = User.Identity?.Name;

        if (!User.IsInRole(AppRoles.Admin) &&
            entity.CreatedBy != currentUserId &&
            entity.CurrentAssignedUserId != currentUserId)
        {
            TempData["ErrorMessage"] = "You are not authorized to update this case.";
            return RedirectToAction(nameof(Index));
        }

        var oldCaseSource = entity.CaseSource;
        var oldReceiptDate = entity.ReceiptDate.ToString("dd-MMM-yyyy");
        var oldInitialReporterName = entity.InitialReporterName;
        var oldInitialPatientIdentifier = entity.InitialPatientIdentifier;
        var oldInitialProductName = entity.InitialProductName;
        var oldInitialEventTerm = entity.InitialEventTerm;

        var oldIsPatientIdentifiable = entity.IsPatientIdentifiable ? "Yes" : "No";
        var oldIsReporterIdentifiable = entity.IsReporterIdentifiable ? "Yes" : "No";
        var oldIsSuspectProductAvailable = entity.IsSuspectProductAvailable ? "Yes" : "No";
        var oldIsAdverseEventAvailable = entity.IsAdverseEventAvailable ? "Yes" : "No";

        var oldIsSerious = entity.IsSerious ? "Yes" : "No";
        var oldIsValidCase = entity.IsValidCase ? "Yes" : "No";
        var oldStatus = entity.Status.ToString();
        var oldDueDate = entity.DueDate?.ToString("dd-MMM-yyyy");
        var oldNarrative = entity.Narrative;
        var oldRemarks = entity.Remarks;

        var isValidCase =
            model.IsPatientIdentifiable &&
            model.IsReporterIdentifiable &&
            model.IsSuspectProductAvailable &&
            model.IsAdverseEventAvailable;

        var dueDate = CalculateDueDate(model.ReceiptDate, model.IsSerious);



        entity.CaseSource = model.CaseSource.Trim();
        entity.ReceiptDate = model.ReceiptDate.Date;

        entity.InitialReporterName = model.InitialReporterName?.Trim();
        entity.InitialPatientIdentifier = model.InitialPatientIdentifier?.Trim();
        entity.InitialProductName = model.InitialProductName?.Trim();
        entity.InitialEventTerm = model.InitialEventTerm?.Trim();

        entity.IsPatientIdentifiable = model.IsPatientIdentifiable;
        entity.IsReporterIdentifiable = model.IsReporterIdentifiable;
        entity.IsSuspectProductAvailable = model.IsSuspectProductAvailable;
        entity.IsAdverseEventAvailable = model.IsAdverseEventAvailable;

        entity.IsValidCase = isValidCase;
        entity.IsSerious = model.IsSerious;
        entity.DueDate = dueDate;

        entity.Narrative = ToPlainText(model.Narrative);
        entity.Remarks = model.Remarks?.Trim();

        if (!isValidCase)
        {
            entity.Status = PvCaseStatus.InvalidFollowUpRequired;
        }
        else if (entity.Status == PvCaseStatus.InvalidFollowUpRequired ||
                 entity.Status == PvCaseStatus.Draft)
        {
            entity.Status = PvCaseStatus.DataEntryInProgress;
        }

        entity.ModifiedBy = currentUserId;
        entity.ModifiedOnUtc = DateTime.UtcNow;

        AddAuditIfChanged(entity, "CaseSource", oldCaseSource, entity.CaseSource, model.ChangeReason, currentUserId, currentUserName);
        AddAuditIfChanged(entity, "ReceiptDate", oldReceiptDate, entity.ReceiptDate.ToString("dd-MMM-yyyy"), model.ChangeReason, currentUserId, currentUserName);

        AddAuditIfChanged(entity, "InitialReporterName", oldInitialReporterName, entity.InitialReporterName, model.ChangeReason, currentUserId, currentUserName);
        AddAuditIfChanged(entity, "InitialPatientIdentifier", oldInitialPatientIdentifier, entity.InitialPatientIdentifier, model.ChangeReason, currentUserId, currentUserName);
        AddAuditIfChanged(entity, "InitialProductName", oldInitialProductName, entity.InitialProductName, model.ChangeReason, currentUserId, currentUserName);
        AddAuditIfChanged(entity, "InitialEventTerm", oldInitialEventTerm, entity.InitialEventTerm, model.ChangeReason, currentUserId, currentUserName);

        AddAuditIfChanged(entity, "IsPatientIdentifiable", oldIsPatientIdentifiable, entity.IsPatientIdentifiable ? "Yes" : "No", model.ChangeReason, currentUserId, currentUserName);
        AddAuditIfChanged(entity, "IsReporterIdentifiable", oldIsReporterIdentifiable, entity.IsReporterIdentifiable ? "Yes" : "No", model.ChangeReason, currentUserId, currentUserName);
        AddAuditIfChanged(entity, "IsSuspectProductAvailable", oldIsSuspectProductAvailable, entity.IsSuspectProductAvailable ? "Yes" : "No", model.ChangeReason, currentUserId, currentUserName);
        AddAuditIfChanged(entity, "IsAdverseEventAvailable", oldIsAdverseEventAvailable, entity.IsAdverseEventAvailable ? "Yes" : "No", model.ChangeReason, currentUserId, currentUserName);

        AddAuditIfChanged(entity, "IsSerious", oldIsSerious, entity.IsSerious ? "Yes" : "No", model.ChangeReason, currentUserId, currentUserName);
        AddAuditIfChanged(entity, "IsValidCase", oldIsValidCase, entity.IsValidCase ? "Yes" : "No", model.ChangeReason, currentUserId, currentUserName);
        AddAuditIfChanged(entity, "Status", oldStatus, entity.Status.ToString(), model.ChangeReason, currentUserId, currentUserName);
        AddAuditIfChanged(entity, "DueDate", oldDueDate, entity.DueDate?.ToString("dd-MMM-yyyy"), model.ChangeReason, currentUserId, currentUserName);

        AddAuditIfChanged(entity, "Narrative", oldNarrative, entity.Narrative, model.ChangeReason, currentUserId, currentUserName);
        AddAuditIfChanged(entity, "Remarks", oldRemarks, entity.Remarks, model.ChangeReason, currentUserId, currentUserName);

        AddCaseComment(
            entity.Id,
            AppRoles.PvAssociate,
            "System",
            "Case Updated",
            $"PV case intake details updated. Reason: {model.ChangeReason}",
            currentUserId);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Case {entity.CaseNo} updated successfully.";

        return RedirectToAction(nameof(Details), new { id = entity.Id });
    }

    // ============================================================
    // PRIVATE METHODS
    // ============================================================

    private async Task LoadDropdownsAsync(PvCaseIntakeViewModel model)
    {
        model.CaseSourceOptions = await _context.CommonMasterOptions
            .Where(x =>
                x.MasterType == CommonMasterType.CaseSource &&
                x.IsActive &&
                !x.IsDeleted)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .Select(x => new SelectListItem
            {
                Value = x.Name,
                Text = x.Name
            })
            .ToListAsync();

        model.ProductOptions = await _context.ProductMasters
            .Where(x => x.IsActive && !x.IsDeleted)
            .OrderBy(x => x.ProductName)
            .Select(x => new SelectListItem
            {
                Value = x.ProductName,
                Text = x.ProductName
            })
            .ToListAsync();
    }

    private async Task<string> GenerateCaseNoAsync()
    {
        var year = DateTime.UtcNow.Year;

        var countForYear = await _context.PvCases
            .CountAsync(x => x.CreatedOnUtc.Year == year);

        var nextNo = countForYear + 1;

        return $"INNOPV-{year}-{nextNo:D6}";
    }

    private static DateTime CalculateDueDate(DateTime receiptDate, bool isSerious)
    {
        return isSerious
            ? receiptDate.Date.AddDays(15)
            : receiptDate.Date.AddDays(90);
    }

    private static bool IsCaseReadOnlyForUpdate(PvCaseStatus status)
    {
        return status == PvCaseStatus.CaseClosed
               || status == PvCaseStatus.CaseFinalized
               || status == PvCaseStatus.Submitted
               || status == PvCaseStatus.AcknowledgementPending
               || status == PvCaseStatus.MarkedAsDuplicate
               || status == PvCaseStatus.MarkedAsInvalid;
    }

    private void AddAuditIfChanged(
    PvCase pvCase,
    string fieldName,
    string? oldValue,
    string? newValue,
    string? remarks,
    string? currentUserId,
    string? currentUserName)
    {
        oldValue ??= string.Empty;
        newValue ??= string.Empty;

        if (string.Equals(oldValue.Trim(), newValue.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var audit = new AuditTrail
        {
            PvCaseId = pvCase.Id,
            CaseNo = pvCase.CaseNo,
            EntityName = "PvCase",
            EntityId = pvCase.Id,
            ActionType = "Case Updated",
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

    private static string? ToPlainText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var text = value;

        text = Regex.Replace(text, "<br\\s*/?>", "\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, "</p>", "\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, "<.*?>", string.Empty);

        text = WebUtility.HtmlDecode(text);

        return text.Trim();
    }

    private void AddIntakeValidationIssues(CaseIntakeValidationResult result)
    {
        foreach (var issue in result.BlockingIssues)
        {
            ModelState.AddModelError(issue.FieldName, issue.Message);
        }
    }
}