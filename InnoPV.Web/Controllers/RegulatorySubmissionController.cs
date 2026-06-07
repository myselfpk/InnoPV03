using InnoPV.Domain.Entities;
using InnoPV.Domain.Enums;
using InnoPV.Domain.Identity;
using InnoPV.Infrastructure.Data;
using InnoPV.Web.Models.RegulatorySubmission;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InnoPV.Web.Services.Security;
using InnoPV.Web.Services.SubmissionValidation;
using InnoPV.Web.Services.FileUpload;
using System.Text;

namespace InnoPV.Web.Controllers;

[Authorize(Policy = AuthorizationPolicies.AdminOrPvManagerOrMedicalReviewer)]
public class RegulatorySubmissionController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICaseSecurityService _caseSecurityService;
    private readonly ISubmissionReadinessValidationService _submissionReadinessValidationService;
    private readonly IFileUploadSecurityService _fileUploadSecurityService;

    public RegulatorySubmissionController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ICaseSecurityService caseSecurityService,
        ISubmissionReadinessValidationService submissionReadinessValidationService,
        IFileUploadSecurityService fileUploadSecurityService)
    {
        _context = context;
        _userManager = userManager;
        _caseSecurityService = caseSecurityService;
        _submissionReadinessValidationService = submissionReadinessValidationService;
        _fileUploadSecurityService = fileUploadSecurityService;
    }

    [HttpGet]
    public async Task<IActionResult> Validate(long caseId)
    {
        if (!EnsurePermission(PermissionActions.RegulatorySubmissionValidate, "You are not allowed to validate submission readiness."))
        {
            return RedirectToAction("Index", "CaseInbox");
        }

        var model = await _submissionReadinessValidationService
            .ValidateCaseForSubmissionAsync(caseId);

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ExportValidationReport(long caseId)
    {
        if (!EnsurePermission(PermissionActions.RegulatorySubmissionExportValidation, "You are not allowed to export validation reports."))
        {
            return RedirectToAction("Index", "CaseInbox");
        }

        var model = await _submissionReadinessValidationService
            .ValidateCaseForSubmissionAsync(caseId);

        var csv = new StringBuilder();
        csv.AppendLine("Category,Section,Check,Result,Message");

        foreach (var check in model.RequiredChecks)
        {
            csv.AppendLine($"Required,{EscapeCsv(check.SectionName)},{EscapeCsv(check.CheckDescription)},{(check.IsPassed ? "Passed" : "Failed")},{EscapeCsv(check.Message)}");
        }

        foreach (var check in model.WarningChecks)
        {
            csv.AppendLine($"Warning,{EscapeCsv(check.SectionName)},{EscapeCsv(check.CheckDescription)},{(check.IsPassed ? "Passed" : "Warning")},{EscapeCsv(check.Message)}");
        }

        var fileName = $"{model.CaseNo}-submission-validation-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
        var bytes = Encoding.UTF8.GetBytes(csv.ToString());

        return File(bytes, "text/csv", fileName);
    }

    [HttpGet]
    public async Task<IActionResult> Index(long caseId)
    {
        if (!EnsurePermission(PermissionActions.RegulatorySubmissionView, "You are not allowed to view regulatory submissions."))
        {
            return RedirectToAction("Index", "CaseInbox");
        }

        var pvCase = await _context.PvCases
            .FirstOrDefaultAsync(x => x.Id == caseId && !x.IsDeleted);

        if (pvCase == null)
        {
            TempData["ErrorMessage"] = "Case not found.";
            return RedirectToAction("Index", "CaseInbox");
        }

        if (!CanViewCase(pvCase))
        {
            return RedirectToAction("Index", "CaseInbox");
        }

        var submissions = await _context.CaseRegulatorySubmissions
            .Where(x => x.PvCaseId == caseId && !x.IsDeleted)
            .OrderByDescending(x => x.DueDate)
            .Select(x => new RegulatorySubmissionListItemViewModel
            {
                Id = x.Id,
                PvCaseId = x.PvCaseId,
                SubmissionNo = x.SubmissionNo,
                SubmissionType = x.SubmissionType,
                RecipientAuthority = x.RecipientAuthority,
                SubmissionFormat = x.SubmissionFormat,
                SubmissionStatus = x.SubmissionStatus,
                DueDate = x.DueDate,
                SubmittedDate = x.SubmittedDate,
                AcknowledgementReceivedDate = x.AcknowledgementReceivedDate,
                ReferenceNo = x.ReferenceNo,
                Remarks = x.Remarks,
                OriginalFileName = x.OriginalFileName,
                FilePath = x.FilePath
            })
            .ToListAsync();

        var model = new RegulatorySubmissionIndexViewModel
        {
            PvCaseId = pvCase.Id,
            CaseNo = pvCase.CaseNo,
            CaseStatus = pvCase.Status,
            ProductName = pvCase.InitialProductName,
            EventTerm = pvCase.InitialEventTerm,
            TotalSubmissions = submissions.Count,
            PendingSubmissions = submissions.Count(x => x.SubmissionStatus == RegulatorySubmissionStatus.SubmissionPending),
            SubmittedCount = submissions.Count(x => x.SubmissionStatus == RegulatorySubmissionStatus.Submitted),
            AcknowledgementPendingCount = submissions.Count(x => x.SubmissionStatus == RegulatorySubmissionStatus.AcknowledgementPending),
            OverdueCount = submissions.Count(x => x.IsOverdue),
            Submissions = submissions
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create(long caseId)
    {
        if (!EnsurePermission(PermissionActions.RegulatorySubmissionCreate, "You are not allowed to create regulatory submissions."))
        {
            return RedirectToAction("Index", "CaseInbox");
        }

        var pvCase = await _context.PvCases
            .FirstOrDefaultAsync(x => x.Id == caseId && !x.IsDeleted);

        if (pvCase == null)
        {
            TempData["ErrorMessage"] = "Case not found.";
            return RedirectToAction("Index", "CaseInbox");
        }

        if (!CanProcessCase(pvCase))
        {
            return RedirectToAction("Index", "CaseInbox");
        }

        var model = new RegulatorySubmissionCreateViewModel
        {
            PvCaseId = pvCase.Id,
            CaseNo = pvCase.CaseNo,
            DueDate = pvCase.DueDate ?? DateTime.Today
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RegulatorySubmissionCreateViewModel model)
    {
        if (!EnsurePermission(PermissionActions.RegulatorySubmissionCreate, "You are not allowed to create regulatory submissions."))
        {
            return RedirectToAction("Index", "CaseInbox");
        }

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

        if (!CanProcessCase(pvCase))
        {
            return RedirectToAction("Index", "CaseInbox");
        }

        var readiness = await _submissionReadinessValidationService
            .ValidateCaseForSubmissionAsync(pvCase.Id);

        if (!readiness.CanSubmit)
        {
            TempData["ErrorMessage"] = "Submission is blocked. Please complete submission readiness validation checks.";
            return RedirectToAction(nameof(Validate), new { caseId = pvCase.Id });
        }

        var currentUserId = _userManager.GetUserId(User);
        var currentUserName = User.Identity?.Name;
        var oldStatus = pvCase.Status.ToString();

        var submissionNo = await GenerateSubmissionNoAsync(pvCase.Id, pvCase.CaseNo);

        var entity = new CaseRegulatorySubmission
        {
            PvCaseId = pvCase.Id,
            SubmissionNo = submissionNo,
            SubmissionType = model.SubmissionType.Trim(),
            RecipientAuthority = model.RecipientAuthority.Trim(),
            SubmissionFormat = model.SubmissionFormat.Trim(),
            DueDate = model.DueDate.Date,
            SubmissionStatus = RegulatorySubmissionStatus.SubmissionPending,
            Remarks = model.Remarks?.Trim(),
            CreatedBy = currentUserId,
            CreatedOnUtc = DateTime.UtcNow
        };

        if (model.SubmissionDocument != null)
        {
            await SaveUploadedFileAsync(entity, model.SubmissionDocument, pvCase.Id);
        }

        _context.CaseRegulatorySubmissions.Add(entity);

        pvCase.Status = PvCaseStatus.SubmissionPending;
        pvCase.ModifiedBy = currentUserId;
        pvCase.ModifiedOnUtc = DateTime.UtcNow;

        AddCaseComment(
            pvCase.Id,
            "Regulatory Submission",
            "System",
            "Submission Created",
            $"Regulatory submission created: {submissionNo}. Authority: {model.RecipientAuthority}. Due Date: {model.DueDate:dd-MMM-yyyy}.",
            currentUserId);

        AddAuditTrail(
            pvCase.Id,
            pvCase.CaseNo,
            "CaseRegulatorySubmission",
            null,
            "Submission Created",
            "Status",
            oldStatus,
            pvCase.Status.ToString(),
            $"Submission No: {submissionNo}. {model.Remarks}",
            currentUserId,
            currentUserName);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Regulatory submission {submissionNo} created successfully.";
        return RedirectToAction(nameof(Index), new { caseId = pvCase.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Submit(long id)
    {
        if (!EnsurePermission(PermissionActions.RegulatorySubmissionSubmit, "You are not allowed to submit regulatory submissions."))
        {
            return RedirectToAction("Index", "CaseInbox");
        }

        var submission = await _context.CaseRegulatorySubmissions
            .Include(x => x.PvCase)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (submission == null)
        {
            TempData["ErrorMessage"] = "Submission record not found.";
            return RedirectToAction("Index", "CaseInbox");
        }

        if (!CanViewCase(submission.PvCase))
        {
            return RedirectToAction("Index", "CaseInbox");
        }

        var model = new RegulatorySubmissionSubmitViewModel
        {
            SubmissionId = submission.Id,
            PvCaseId = submission.PvCaseId,
            SubmissionNo = submission.SubmissionNo,
            CaseNo = submission.PvCase.CaseNo,
            SubmittedDate = DateTime.Today,
            ReferenceNo = submission.ReferenceNo,
            Remarks = submission.Remarks,
            IsAcknowledgementExpected = true
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(RegulatorySubmissionSubmitViewModel model)
    {
        if (!EnsurePermission(PermissionActions.RegulatorySubmissionSubmit, "You are not allowed to submit regulatory submissions."))
        {
            return RedirectToAction("Index", "CaseInbox");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var submission = await _context.CaseRegulatorySubmissions
            .Include(x => x.PvCase)
            .FirstOrDefaultAsync(x => x.Id == model.SubmissionId && !x.IsDeleted);

        if (submission == null)
        {
            TempData["ErrorMessage"] = "Submission record not found.";
            return RedirectToAction("Index", "CaseInbox");
        }

        if (!CanProcessCase(submission.PvCase))
        {
            return RedirectToAction("Index", "CaseInbox");
        }

        var readiness = await _submissionReadinessValidationService
            .ValidateCaseForSubmissionAsync(submission.PvCaseId);

        if (!readiness.CanSubmit)
        {
            TempData["ErrorMessage"] = "Submission cannot proceed. Submission readiness validation has blocking failures.";
            return RedirectToAction(nameof(Validate), new { caseId = submission.PvCaseId });
        }

        var currentUserId = _userManager.GetUserId(User);
        var currentUserName = User.Identity?.Name;

        var oldSubmissionStatus = submission.SubmissionStatus.ToString();
        var oldCaseStatus = submission.PvCase.Status.ToString();

        submission.SubmittedDate = model.SubmittedDate.Date;
        submission.ReferenceNo = model.ReferenceNo?.Trim();
        submission.Remarks = model.Remarks?.Trim();
        submission.SubmittedByUserId = currentUserId;
        submission.SubmissionStatus = model.IsAcknowledgementExpected
            ? RegulatorySubmissionStatus.AcknowledgementPending
            : RegulatorySubmissionStatus.Submitted;
        submission.ModifiedBy = currentUserId;
        submission.ModifiedOnUtc = DateTime.UtcNow;

        if (model.SubmissionDocument != null)
        {
            await SaveUploadedFileAsync(submission, model.SubmissionDocument, submission.PvCaseId);
        }

        submission.PvCase.Status = model.IsAcknowledgementExpected
            ? PvCaseStatus.AcknowledgementPending
            : PvCaseStatus.Submitted;

        submission.PvCase.ModifiedBy = currentUserId;
        submission.PvCase.ModifiedOnUtc = DateTime.UtcNow;

        AddCaseComment(
            submission.PvCaseId,
            "Regulatory Submission",
            "System",
            "Submitted",
            $"Submission {submission.SubmissionNo} submitted on {model.SubmittedDate:dd-MMM-yyyy}. Reference No: {model.ReferenceNo}.",
            currentUserId);

        AddAuditTrail(
            submission.PvCaseId,
            submission.PvCase.CaseNo,
            "CaseRegulatorySubmission",
            submission.Id,
            "Submission Submitted",
            "SubmissionStatus",
            oldSubmissionStatus,
            submission.SubmissionStatus.ToString(),
            model.Remarks,
            currentUserId,
            currentUserName);

        AddAuditTrail(
            submission.PvCaseId,
            submission.PvCase.CaseNo,
            "PvCase",
            submission.PvCaseId,
            "Case Status Updated",
            "Status",
            oldCaseStatus,
            submission.PvCase.Status.ToString(),
            $"Submission No: {submission.SubmissionNo}",
            currentUserId,
            currentUserName);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Submission {submission.SubmissionNo} marked as submitted.";
        return RedirectToAction(nameof(Index), new { caseId = submission.PvCaseId });
    }

    [HttpGet]
    public async Task<IActionResult> Acknowledgement(long id)
    {
        if (!EnsurePermission(PermissionActions.RegulatorySubmissionAcknowledge, "You are not allowed to process acknowledgements."))
        {
            return RedirectToAction("Index", "CaseInbox");
        }

        var submission = await _context.CaseRegulatorySubmissions
            .Include(x => x.PvCase)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (submission == null)
        {
            TempData["ErrorMessage"] = "Submission record not found.";
            return RedirectToAction("Index", "CaseInbox");
        }

        if (!CanViewCase(submission.PvCase))
        {
            return RedirectToAction("Index", "CaseInbox");
        }

        var model = new RegulatorySubmissionAcknowledgementViewModel
        {
            SubmissionId = submission.Id,
            PvCaseId = submission.PvCaseId,
            SubmissionNo = submission.SubmissionNo,
            CaseNo = submission.PvCase.CaseNo,
            AcknowledgementReceivedDate = DateTime.Today,
            ReferenceNo = submission.ReferenceNo,
            AcknowledgementRemarks = submission.AcknowledgementRemarks
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Acknowledgement(RegulatorySubmissionAcknowledgementViewModel model)
    {
        if (!EnsurePermission(PermissionActions.RegulatorySubmissionAcknowledge, "You are not allowed to process acknowledgements."))
        {
            return RedirectToAction("Index", "CaseInbox");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var submission = await _context.CaseRegulatorySubmissions
            .Include(x => x.PvCase)
            .FirstOrDefaultAsync(x => x.Id == model.SubmissionId && !x.IsDeleted);

        if (submission == null)
        {
            TempData["ErrorMessage"] = "Submission record not found.";
            return RedirectToAction("Index", "CaseInbox");
        }

        if (!CanProcessCase(submission.PvCase))
        {
            return RedirectToAction("Index", "CaseInbox");
        }

        var currentUserId = _userManager.GetUserId(User);
        var currentUserName = User.Identity?.Name;

        var oldSubmissionStatus = submission.SubmissionStatus.ToString();
        var oldCaseStatus = submission.PvCase.Status.ToString();

        submission.AcknowledgementReceivedDate = model.AcknowledgementReceivedDate.Date;
        submission.ReferenceNo = model.ReferenceNo?.Trim();
        submission.AcknowledgementRemarks = model.AcknowledgementRemarks?.Trim();
        submission.SubmissionStatus = RegulatorySubmissionStatus.AcknowledgementReceived;
        submission.ModifiedBy = currentUserId;
        submission.ModifiedOnUtc = DateTime.UtcNow;

        if (model.AcknowledgementDocument != null)
        {
            await SaveUploadedFileAsync(submission, model.AcknowledgementDocument, submission.PvCaseId);
        }

        submission.PvCase.Status = PvCaseStatus.CaseClosed;
        submission.PvCase.ModifiedBy = currentUserId;
        submission.PvCase.ModifiedOnUtc = DateTime.UtcNow;

        AddCaseComment(
            submission.PvCaseId,
            "Regulatory Submission",
            "System",
            "Acknowledgement Received",
            $"Acknowledgement received for submission {submission.SubmissionNo} on {model.AcknowledgementReceivedDate:dd-MMM-yyyy}. Reference No: {model.ReferenceNo}.",
            currentUserId);

        AddAuditTrail(
            submission.PvCaseId,
            submission.PvCase.CaseNo,
            "CaseRegulatorySubmission",
            submission.Id,
            "Acknowledgement Received",
            "SubmissionStatus",
            oldSubmissionStatus,
            submission.SubmissionStatus.ToString(),
            model.AcknowledgementRemarks,
            currentUserId,
            currentUserName);

        AddAuditTrail(
            submission.PvCaseId,
            submission.PvCase.CaseNo,
            "PvCase",
            submission.PvCaseId,
            "Case Status Updated",
            "Status",
            oldCaseStatus,
            submission.PvCase.Status.ToString(),
            $"Acknowledgement received for submission {submission.SubmissionNo}.",
            currentUserId,
            currentUserName);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Acknowledgement recorded for submission {submission.SubmissionNo}.";
        return RedirectToAction(nameof(Index), new { caseId = submission.PvCaseId });
    }

    [HttpGet]
    public async Task<IActionResult> DownloadDocument(long id)
    {
        if (!EnsurePermission(PermissionActions.RegulatorySubmissionDownload, "You are not allowed to download submission documents."))
        {
            return RedirectToAction("Index", "CaseInbox");
        }

        var submission = await _context.CaseRegulatorySubmissions
            .Include(x => x.PvCase)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (submission == null || string.IsNullOrWhiteSpace(submission.FilePath))
        {
            TempData["ErrorMessage"] = "Document not found.";
            return RedirectToAction("Index", "CaseInbox");
        }

        if (!CanViewCase(submission.PvCase))
        {
            return RedirectToAction("Index", "CaseInbox");
        }

        var physicalPath = _fileUploadSecurityService.ResolvePrivateUploadPath(
            submission.FilePath,
            "regulatory-submissions");

        if (physicalPath == null || !System.IO.File.Exists(physicalPath))
        {
            TempData["ErrorMessage"] = "Document file not found on server.";
            return RedirectToAction(nameof(Index), new { caseId = submission.PvCaseId });
        }

        var bytes = await System.IO.File.ReadAllBytesAsync(physicalPath);

        return File(
            bytes,
            submission.ContentType ?? "application/octet-stream",
            submission.OriginalFileName ?? "submission-document");
    }

    private async Task<string> GenerateSubmissionNoAsync(long pvCaseId, string caseNo)
    {
        var count = await _context.CaseRegulatorySubmissions
            .CountAsync(x => x.PvCaseId == pvCaseId && !x.IsDeleted);

        var nextNo = count + 1;

        return $"{caseNo}-SUB{nextNo:00}";
    }

    private async Task SaveUploadedFileAsync(
        CaseRegulatorySubmission submission,
        IFormFile file,
        long pvCaseId)
    {
        if (file == null || file.Length == 0)
        {
            return;
        }

        const long maxFileSize = 10 * 1024 * 1024;
        var fileValidation = await _fileUploadSecurityService.ValidateDocumentAsync(file, maxFileSize);

        if (!fileValidation.IsValid)
        {
            throw new InvalidOperationException(fileValidation.ErrorMessage ?? "Invalid file.");
        }

        var uploadFolder = _fileUploadSecurityService.GetPrivateUploadFolder(
            "regulatory-submissions",
            pvCaseId.ToString());

        if (!Directory.Exists(uploadFolder))
        {
            Directory.CreateDirectory(uploadFolder);
        }

        var storedFileName = $"{Guid.NewGuid():N}{fileValidation.Extension}";
        var physicalPath = Path.Combine(uploadFolder, storedFileName);

        await using (var stream = new FileStream(physicalPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        submission.OriginalFileName = fileValidation.OriginalFileName;
        submission.StoredFileName = storedFileName;
        submission.FilePath = _fileUploadSecurityService.ToStoredFilePath(
            "regulatory-submissions",
            pvCaseId.ToString(),
            storedFileName);
        submission.ContentType = fileValidation.ContentType;
        submission.FileSizeBytes = file.Length;
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

    private bool CanProcessCase(PvCase pvCase)
    {
        var currentUserId = _userManager.GetUserId(User);

        if (_caseSecurityService.CanProcessWorkflow(pvCase, User, currentUserId))
        {
            return true;
        }

        TempData["ErrorMessage"] = "You are not allowed to process this case.";
        return false;
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

    private static string EscapeCsv(string? value)
    {
        var safe = (value ?? string.Empty).Replace("\"", "\"\"");
        return $"\"{safe}\"";
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
