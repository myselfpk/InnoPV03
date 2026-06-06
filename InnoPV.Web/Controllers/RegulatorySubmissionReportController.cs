using InnoPV.Domain.Enums;
using InnoPV.Domain.Identity;
using InnoPV.Infrastructure.Data;
using InnoPV.Web.Models.RegulatorySubmissionReport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace InnoPV.Web.Controllers;

[Authorize(Roles = AppRoles.Admin + "," + AppRoles.PvManager)]
public class RegulatorySubmissionReportController : Controller
{
    private readonly ApplicationDbContext _context;

    public RegulatorySubmissionReportController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] RegulatorySubmissionReportFilterViewModel filters)
    {
        await LoadDropdownsAsync(filters);

        var submissions = await GetFilteredSubmissionsAsync(filters);

        var model = new RegulatorySubmissionReportViewModel
        {
            Filters = filters,
            Submissions = submissions,
            TotalCount = submissions.Count,
            PendingCount = submissions.Count(x => x.SubmissionStatus == RegulatorySubmissionStatus.SubmissionPending),
            SubmittedCount = submissions.Count(x => x.SubmissionStatus == RegulatorySubmissionStatus.Submitted),
            AcknowledgementPendingCount = submissions.Count(x => x.SubmissionStatus == RegulatorySubmissionStatus.AcknowledgementPending),
            AcknowledgementReceivedCount = submissions.Count(x => x.SubmissionStatus == RegulatorySubmissionStatus.AcknowledgementReceived),
            OverdueCount = submissions.Count(x => x.IsOverdue),
            RejectedCount = submissions.Count(x => x.SubmissionStatus == RegulatorySubmissionStatus.Rejected),
            CancelledCount = submissions.Count(x => x.SubmissionStatus == RegulatorySubmissionStatus.Cancelled)
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ExportCsv([FromQuery] RegulatorySubmissionReportFilterViewModel filters)
    {
        var submissions = await GetFilteredSubmissionsAsync(filters);

        var csv = new StringBuilder();

        csv.AppendLine("Case No,Case Status,Product,Event,Submission No,Submission Type,Recipient Authority,Submission Format,Submission Status,Due Date,Submitted Date,Acknowledgement Received Date,Reference No,Overdue,Remarks,Created On");

        foreach (var item in submissions)
        {
            csv.AppendLine(string.Join(",",
                EscapeCsv(item.CaseNo),
                EscapeCsv(item.CaseStatus.ToString()),
                EscapeCsv(item.ProductName),
                EscapeCsv(item.EventTerm),
                EscapeCsv(item.SubmissionNo),
                EscapeCsv(item.SubmissionType),
                EscapeCsv(item.RecipientAuthority),
                EscapeCsv(item.SubmissionFormat),
                EscapeCsv(item.SubmissionStatus.ToString()),
                EscapeCsv(item.DueDate.ToString("dd-MMM-yyyy")),
                EscapeCsv(item.SubmittedDate.HasValue ? item.SubmittedDate.Value.ToString("dd-MMM-yyyy") : ""),
                EscapeCsv(item.AcknowledgementReceivedDate.HasValue ? item.AcknowledgementReceivedDate.Value.ToString("dd-MMM-yyyy") : ""),
                EscapeCsv(item.ReferenceNo),
                EscapeCsv(item.IsOverdue ? "Yes" : "No"),
                EscapeCsv(item.Remarks),
                EscapeCsv(item.CreatedOnUtc.ToString("dd-MMM-yyyy HH:mm"))
            ));
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        var fileName = $"Regulatory_Submission_Report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";

        return File(bytes, "text/csv", fileName);
    }

    private async Task<List<RegulatorySubmissionReportListItemViewModel>> GetFilteredSubmissionsAsync(
        RegulatorySubmissionReportFilterViewModel filters)
    {
        var query = _context.CaseRegulatorySubmissions
            .Include(x => x.PvCase)
            .Where(x => !x.IsDeleted && !x.PvCase.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filters.CaseNo))
        {
            var caseNo = filters.CaseNo.Trim();
            query = query.Where(x => x.PvCase.CaseNo.Contains(caseNo));
        }

        if (!string.IsNullOrWhiteSpace(filters.SubmissionNo))
        {
            var submissionNo = filters.SubmissionNo.Trim();
            query = query.Where(x => x.SubmissionNo.Contains(submissionNo));
        }

        if (!string.IsNullOrWhiteSpace(filters.SubmissionType))
        {
            query = query.Where(x => x.SubmissionType == filters.SubmissionType);
        }

        if (!string.IsNullOrWhiteSpace(filters.RecipientAuthority))
        {
            query = query.Where(x => x.RecipientAuthority == filters.RecipientAuthority);
        }

        if (!string.IsNullOrWhiteSpace(filters.SubmissionFormat))
        {
            query = query.Where(x => x.SubmissionFormat == filters.SubmissionFormat);
        }

        if (filters.SubmissionStatus.HasValue)
        {
            query = query.Where(x => x.SubmissionStatus == filters.SubmissionStatus.Value);
        }

        if (filters.DueDateFrom.HasValue)
        {
            var fromDate = filters.DueDateFrom.Value.Date;
            query = query.Where(x => x.DueDate >= fromDate);
        }

        if (filters.DueDateTo.HasValue)
        {
            var toDate = filters.DueDateTo.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(x => x.DueDate <= toDate);
        }

        if (filters.SubmittedDateFrom.HasValue)
        {
            var fromDate = filters.SubmittedDateFrom.Value.Date;
            query = query.Where(x => x.SubmittedDate.HasValue && x.SubmittedDate.Value >= fromDate);
        }

        if (filters.SubmittedDateTo.HasValue)
        {
            var toDate = filters.SubmittedDateTo.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(x => x.SubmittedDate.HasValue && x.SubmittedDate.Value <= toDate);
        }

        var submissions = await query
            .OrderBy(x => x.DueDate)
            .ThenByDescending(x => x.CreatedOnUtc)
            .Select(x => new RegulatorySubmissionReportListItemViewModel
            {
                Id = x.Id,
                PvCaseId = x.PvCaseId,
                CaseNo = x.PvCase.CaseNo,
                CaseStatus = x.PvCase.Status,
                ProductName = x.PvCase.InitialProductName,
                EventTerm = x.PvCase.InitialEventTerm,
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
                CreatedOnUtc = x.CreatedOnUtc
            })
            .ToListAsync();

        if (filters.IsOverdue.HasValue)
        {
            submissions = submissions
                .Where(x => x.IsOverdue == filters.IsOverdue.Value)
                .ToList();
        }

        return submissions;
    }

    private Task LoadDropdownsAsync(RegulatorySubmissionReportFilterViewModel filters)
    {
        filters.SubmissionStatusOptions = Enum.GetValues(typeof(RegulatorySubmissionStatus))
            .Cast<RegulatorySubmissionStatus>()
            .Select(x => new SelectListItem
            {
                Value = x.ToString(),
                Text = x.ToString()
            })
            .ToList();

        filters.SubmissionTypeOptions = new List<SelectListItem>
        {
            new() { Value = "Initial Submission", Text = "Initial Submission" },
            new() { Value = "Follow-up Submission", Text = "Follow-up Submission" },
            new() { Value = "Final Submission", Text = "Final Submission" },
            new() { Value = "Other", Text = "Other" }
        };

        filters.RecipientAuthorityOptions = new List<SelectListItem>
        {
            new() { Value = "IPC / PvPI", Text = "IPC / PvPI" },
            new() { Value = "CDSCO", Text = "CDSCO" },
            new() { Value = "Sponsor", Text = "Sponsor" },
            new() { Value = "Ethics Committee", Text = "Ethics Committee" },
            new() { Value = "Regulatory Authority", Text = "Regulatory Authority" },
            new() { Value = "Other", Text = "Other" }
        };

        filters.SubmissionFormatOptions = new List<SelectListItem>
        {
            new() { Value = "Email", Text = "Email" },
            new() { Value = "Portal", Text = "Portal" },
            new() { Value = "XML", Text = "XML" },
            new() { Value = "PDF", Text = "PDF" },
            new() { Value = "Excel", Text = "Excel" },
            new() { Value = "Other", Text = "Other" }
        };

        return Task.CompletedTask;
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