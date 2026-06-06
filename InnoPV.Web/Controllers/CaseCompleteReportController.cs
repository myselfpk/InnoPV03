using InnoPV.Domain.Enums;
using InnoPV.Domain.Identity;
using InnoPV.Infrastructure.Data;
using InnoPV.Web.Models.CaseCompleteReport;
using InnoPV.Web.Services.CaseCompleteReport;
using InnoPV.Web.Services.CaseCompleteReport.Pdf;
using InnoPV.Web.Services.CaseCompleteReport.Word;
using InnoPV.Web.Services.CaseCompleteReport.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;

namespace InnoPV.Web.Controllers;

[Authorize(Policy = AuthorizationPolicies.AuthenticatedPvUser)]
public class CaseCompleteReportController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICaseCompleteReportService _caseCompleteReportService;

    public CaseCompleteReportController(
        ApplicationDbContext context,
        ICaseCompleteReportService caseCompleteReportService)
    {
        _context = context;
        _caseCompleteReportService = caseCompleteReportService;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] CaseCompleteReportFilterViewModel filters)
    {
        LoadDropdowns(filters);

        var cases = await GetFilteredCasesAsync(filters);

        var model = new CaseCompleteReportIndexViewModel
        {
            Filters = filters,
            Cases = cases,
            TotalCount = cases.Count,
            SeriousCount = cases.Count(x => x.IsSerious),
            NonSeriousCount = cases.Count(x => !x.IsSerious),
            ValidCount = cases.Count(x => x.IsValidCase),
            InvalidCount = cases.Count(x => !x.IsValidCase),
            ClosedCount = cases.Count(x =>
                x.Status == PvCaseStatus.CaseClosed ||
                x.Status == PvCaseStatus.CaseFinalized ||
                x.Status == PvCaseStatus.Submitted)
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ExportPdf(long caseId)
    {
        var model = await _caseCompleteReportService.GetCaseCompleteReportAsync(caseId);

        if (model == null)
        {
            TempData["ErrorMessage"] = "Case not found.";
            return RedirectToAction("Index", "CaseInbox");
        }

        var document = new CaseCompletePdfDocument(model);

        var pdfBytes = document.GeneratePdf();

        var fileName = $"{SafeFileName(model.CaseNo)}_Complete_Case_Report.pdf";

        return File(pdfBytes, "application/pdf", fileName);
    }

    [HttpGet]
    public async Task<IActionResult> ExportWord(long caseId)
    {
        var model = await _caseCompleteReportService.GetCaseCompleteReportAsync(caseId);

        if (model == null)
        {
            TempData["ErrorMessage"] = "Case not found.";
            return RedirectToAction("Index", "CaseInbox");
        }

        var wordService = new CaseCompleteWordExportService();

        var wordBytes = wordService.Generate(model);

        var fileName = $"{SafeFileName(model.CaseNo)}_Complete_Case_Report.docx";

        return File(
            wordBytes,
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            fileName);
    }

    [HttpGet]
    public async Task<IActionResult> ExportExcel(long caseId)
    {
        var model = await _caseCompleteReportService.GetCaseCompleteReportAsync(caseId);

        if (model == null)
        {
            TempData["ErrorMessage"] = "Case not found.";
            return RedirectToAction("Index", "CaseInbox");
        }

        var excelService = new CaseCompleteExcelExportService();

        var excelBytes = excelService.Generate(model);

        var fileName = $"{SafeFileName(model.CaseNo)}_Complete_Case_Report.xlsx";

        return File(
            excelBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    private async Task<List<CaseCompleteReportListItemViewModel>> GetFilteredCasesAsync(
        CaseCompleteReportFilterViewModel filters)
    {
        var query = _context.PvCases
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filters.CaseNo))
        {
            var caseNo = filters.CaseNo.Trim();
            query = query.Where(x => x.CaseNo.Contains(caseNo));
        }

        if (!string.IsNullOrWhiteSpace(filters.ProductName))
        {
            var productName = filters.ProductName.Trim();
            query = query.Where(x => x.InitialProductName != null &&
                                     x.InitialProductName.Contains(productName));
        }

        if (!string.IsNullOrWhiteSpace(filters.EventTerm))
        {
            var eventTerm = filters.EventTerm.Trim();
            query = query.Where(x => x.InitialEventTerm != null &&
                                     x.InitialEventTerm.Contains(eventTerm));
        }

        if (filters.Status.HasValue)
        {
            query = query.Where(x => x.Status == filters.Status.Value);
        }

        if (filters.IsSerious.HasValue)
        {
            query = query.Where(x => x.IsSerious == filters.IsSerious.Value);
        }

        if (filters.IsValidCase.HasValue)
        {
            query = query.Where(x => x.IsValidCase == filters.IsValidCase.Value);
        }

        if (filters.ReceiptDateFrom.HasValue)
        {
            var fromDate = filters.ReceiptDateFrom.Value.Date;
            query = query.Where(x => x.ReceiptDate >= fromDate);
        }

        if (filters.ReceiptDateTo.HasValue)
        {
            var toDate = filters.ReceiptDateTo.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(x => x.ReceiptDate <= toDate);
        }

        return await query
            .OrderByDescending(x => x.CreatedOnUtc)
            .Select(x => new CaseCompleteReportListItemViewModel
            {
                Id = x.Id,
                CaseNo = x.CaseNo,
                CaseSource = x.CaseSource,
                ReceiptDate = x.ReceiptDate,
                Status = x.Status,
                IsValidCase = x.IsValidCase,
                IsSerious = x.IsSerious,
                DueDate = x.DueDate,
                InitialReporterName = x.InitialReporterName,
                InitialPatientIdentifier = x.InitialPatientIdentifier,
                InitialProductName = x.InitialProductName,
                InitialEventTerm = x.InitialEventTerm,
                CurrentAssignedRole = x.CurrentAssignedRole,
                CreatedOnUtc = x.CreatedOnUtc
            })
            .ToListAsync();
    }

    private static void LoadDropdowns(CaseCompleteReportFilterViewModel filters)
    {
        filters.StatusOptions = Enum.GetValues(typeof(PvCaseStatus))
            .Cast<PvCaseStatus>()
            .Select(x => new SelectListItem
            {
                Value = x.ToString(),
                Text = x.ToString()
            })
            .ToList();
    }

    private static string SafeFileName(string value)
    {
        foreach (var invalidChar in Path.GetInvalidFileNameChars())
        {
            value = value.Replace(invalidChar, '_');
        }

        return value;
    }
}