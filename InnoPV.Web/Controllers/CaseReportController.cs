using InnoPV.Domain.Enums;
using InnoPV.Domain.Identity;
using InnoPV.Infrastructure.Data;
using InnoPV.Web.Models.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace InnoPV.Web.Controllers;

[Authorize(Policy = AuthorizationPolicies.AdminOrPvManager)]
public class CaseReportController : Controller
{
    private readonly ApplicationDbContext _context;

    public CaseReportController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] CaseReportFilterViewModel filters)
    {
        await LoadDropdownsAsync(filters);

        var cases = await GetFilteredCasesAsync(filters);

        var model = new CaseReportViewModel
        {
            Filters = filters,
            Cases = cases,
            TotalCount = cases.Count,
            ValidCount = cases.Count(x => x.IsValidCase),
            InvalidCount = cases.Count(x => !x.IsValidCase),
            SeriousCount = cases.Count(x => x.IsSerious),
            NonSeriousCount = cases.Count(x => !x.IsSerious),
            ClosedCount = cases.Count(x => x.Status == PvCaseStatus.CaseClosed),
            OverdueCount = cases.Count(x => x.IsOverdue)
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ExportCsv([FromQuery] CaseReportFilterViewModel filters)
    {
        var cases = await GetFilteredCasesAsync(filters);

        var csv = new StringBuilder();

        csv.AppendLine("Case No,Case Source,Receipt Date,Reporter,Patient Identifier,Product,Event,Valid Case,Serious,Due Date,Status,Assigned Role,Assigned User,Created On,Overdue");

        foreach (var item in cases)
        {
            csv.AppendLine(string.Join(",",
                EscapeCsv(item.CaseNo),
                EscapeCsv(item.CaseSource),
                EscapeCsv(item.ReceiptDate.ToString("dd-MMM-yyyy")),
                EscapeCsv(item.InitialReporterName),
                EscapeCsv(item.InitialPatientIdentifier),
                EscapeCsv(item.InitialProductName),
                EscapeCsv(item.InitialEventTerm),
                EscapeCsv(item.IsValidCase ? "Yes" : "No"),
                EscapeCsv(item.IsSerious ? "Yes" : "No"),
                EscapeCsv(item.DueDate.HasValue ? item.DueDate.Value.ToString("dd-MMM-yyyy") : ""),
                EscapeCsv(item.Status.ToString()),
                EscapeCsv(item.CurrentAssignedRole),
                EscapeCsv(item.CurrentAssignedUserName),
                EscapeCsv(item.CreatedOnUtc.ToString("dd-MMM-yyyy HH:mm")),
                EscapeCsv(item.IsOverdue ? "Yes" : "No")
            ));
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        var fileName = $"PV_Case_Report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";

        return File(bytes, "text/csv", fileName);
    }

    private async Task<List<CaseReportListItemViewModel>> GetFilteredCasesAsync(CaseReportFilterViewModel filters)
    {
        var query = _context.PvCases
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filters.CaseNo))
        {
            var caseNo = filters.CaseNo.Trim();
            query = query.Where(x => x.CaseNo.Contains(caseNo));
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

        if (filters.Status.HasValue)
        {
            query = query.Where(x => x.Status == filters.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(filters.AssignedRole))
        {
            query = query.Where(x => x.CurrentAssignedRole == filters.AssignedRole);
        }

        if (!string.IsNullOrWhiteSpace(filters.ProductName))
        {
            query = query.Where(x => x.InitialProductName == filters.ProductName);
        }

        if (filters.IsSerious.HasValue)
        {
            query = query.Where(x => x.IsSerious == filters.IsSerious.Value);
        }

        if (filters.IsValidCase.HasValue)
        {
            query = query.Where(x => x.IsValidCase == filters.IsValidCase.Value);
        }

        var cases = await query
            .OrderByDescending(x => x.CreatedOnUtc)
            .Select(x => new CaseReportListItemViewModel
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

        return cases;
    }

    private async Task LoadDropdownsAsync(CaseReportFilterViewModel filters)
    {
        filters.StatusOptions = Enum.GetValues(typeof(PvCaseStatus))
            .Cast<PvCaseStatus>()
            .Select(x => new SelectListItem
            {
                Value = x.ToString(),
                Text = x.ToString()
            })
            .ToList();

        filters.RoleOptions = new List<SelectListItem>
        {
            new() { Value = AppRoles.PvAssociate, Text = AppRoles.PvAssociate },
            new() { Value = AppRoles.PvManager, Text = AppRoles.PvManager },
            new() { Value = AppRoles.MedicalReviewer, Text = AppRoles.MedicalReviewer }
        };

        filters.ProductOptions = await _context.ProductMasters
            .Where(x => x.IsActive && !x.IsDeleted)
            .OrderBy(x => x.ProductName)
            .Select(x => new SelectListItem
            {
                Value = x.ProductName,
                Text = x.ProductName
            })
            .ToListAsync();
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "";
        }

        value = value.Replace("\"", "\"\"");

        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value}\"";
        }

        return value;
    }
}