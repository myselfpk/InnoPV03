using InnoPV.Domain.Enums;
using InnoPV.Domain.Identity;
using InnoPV.Infrastructure.Data;
using InnoPV.Web.Models.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InnoPV.Web.Controllers;

[Authorize(Policy = AuthorizationPolicies.AuthenticatedPvUser)]
public class PvDashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public PvDashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var today = DateTime.UtcNow.Date;
        var next7Days = today.AddDays(7);
        var last12MonthsStart = new DateTime(today.Year, today.Month, 1).AddMonths(-11);

        var cases = await _context.PvCases
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .Select(x => new
            {
                x.Id,
                x.CaseNo,
                x.InitialProductName,
                x.InitialEventTerm,
                x.IsSerious,
                x.IsValidCase,
                x.ReceiptDate,
                x.DueDate,
                x.Status,
                x.CurrentAssignedRole,
                x.CurrentAssignedUserId,
                x.CreatedOnUtc
            })
            .ToListAsync();

        var assignedUserIds = cases
            .Where(x => !string.IsNullOrWhiteSpace(x.CurrentAssignedUserId))
            .Select(x => x.CurrentAssignedUserId!)
            .Distinct()
            .ToList();

        var users = await _context.Users
            .AsNoTracking()
            .Where(x => assignedUserIds.Contains(x.Id))
            .Select(x => new UserLookupItem
            {
                Id = x.Id,
                UserName = x.UserName,
                Email = x.Email
            })
            .ToListAsync();

        var closedStatuses = new[]
        {
            PvCaseStatus.CaseClosed,
            PvCaseStatus.CaseFinalized,
            PvCaseStatus.Submitted
        };

        var totalCount = cases.Count;
        var openCount = cases.Count(x => !closedStatuses.Contains(x.Status));
        var closedCount = cases.Count(x => closedStatuses.Contains(x.Status));
        var seriousCount = cases.Count(x => x.IsSerious);
        var validCount = cases.Count(x => x.IsValidCase);
        var invalidCount = cases.Count(x => !x.IsValidCase);

        var overdueCases = cases
            .Where(x =>
                x.DueDate.HasValue &&
                x.DueDate.Value.Date < today &&
                !closedStatuses.Contains(x.Status))
            .OrderBy(x => x.DueDate)
            .Select(x => ToSlaCaseItem(x.Id, x.CaseNo, x.InitialProductName, x.InitialEventTerm, x.IsSerious,
                x.ReceiptDate, x.DueDate, x.Status, x.CurrentAssignedRole, x.CurrentAssignedUserId, users))
            .ToList();

        var dueSoonCases = cases
            .Where(x =>
                x.DueDate.HasValue &&
                x.DueDate.Value.Date >= today &&
                x.DueDate.Value.Date <= next7Days &&
                !closedStatuses.Contains(x.Status))
            .OrderBy(x => x.DueDate)
            .Select(x => ToSlaCaseItem(x.Id, x.CaseNo, x.InitialProductName, x.InitialEventTerm, x.IsSerious,
                x.ReceiptDate, x.DueDate, x.Status, x.CurrentAssignedRole, x.CurrentAssignedUserId, users))
            .ToList();

        var statusChart = cases
            .GroupBy(x => x.Status.ToString())
            .OrderBy(x => x.Key)
            .Select(x => new DashboardChartItemViewModel
            {
                Label = x.Key,
                Count = x.Count()
            })
            .ToList();

        var roleChart = cases
            .GroupBy(x => string.IsNullOrWhiteSpace(x.CurrentAssignedRole) ? "Not Assigned" : x.CurrentAssignedRole)
            .OrderBy(x => x.Key)
            .Select(x => new DashboardChartItemViewModel
            {
                Label = x.Key,
                Count = x.Count()
            })
            .ToList();

        var monthlyTrend = new List<DashboardChartItemViewModel>();

        for (var i = 0; i < 12; i++)
        {
            var monthStart = last12MonthsStart.AddMonths(i);
            var monthEnd = monthStart.AddMonths(1);

            monthlyTrend.Add(new DashboardChartItemViewModel
            {
                Label = monthStart.ToString("MMM-yyyy"),
                Count = cases.Count(x => x.CreatedOnUtc >= monthStart && x.CreatedOnUtc < monthEnd)
            });
        }

        var model = new PvDashboardViewModel
        {
            Cards = new List<DashboardCountCardViewModel>
            {
                new()
                {
                    Title = "Total Cases",
                    Count = totalCount,
                    Description = "All PV cases in system"
                },
                new()
                {
                    Title = "Open Cases",
                    Count = openCount,
                    Description = "Cases not yet closed/finalized"
                },
                new()
                {
                    Title = "Closed Cases",
                    Count = closedCount,
                    Description = "Closed/finalized/submitted cases"
                },
                new()
                {
                    Title = "Serious Cases",
                    Count = seriousCount,
                    Description = "Cases marked as serious"
                },
                new()
                {
                    Title = "Valid Cases",
                    Count = validCount,
                    Description = "Cases meeting validity criteria"
                },
                new()
                {
                    Title = "Invalid Cases",
                    Count = invalidCount,
                    Description = "Cases requiring follow-up or invalid"
                }
            },
            StatusChart = statusChart,
            RoleChart = roleChart,
            MonthlyTrendChart = monthlyTrend,
            OverdueCases = overdueCases.Take(20).ToList(),
            DueSoonCases = dueSoonCases.Take(20).ToList(),
            SeriousOpenCount = cases.Count(x => x.IsSerious && !closedStatuses.Contains(x.Status)),
            NonSeriousOpenCount = cases.Count(x => !x.IsSerious && !closedStatuses.Contains(x.Status)),
            DueWithin7DaysCount = dueSoonCases.Count,
            OverdueCount = overdueCases.Count
        };

        return View(model);
    }

    private static SlaCaseItemViewModel ToSlaCaseItem(
    long id,
    string caseNo,
    string? productName,
    string? eventTerm,
    bool isSerious,
    DateTime receiptDate,
    DateTime? dueDate,
    PvCaseStatus status,
    string? assignedRole,
    string? assignedUserId,
    List<UserLookupItem> users)
    {
        string? assignedUserName = null;

        if (!string.IsNullOrWhiteSpace(assignedUserId))
        {
            var user = users.FirstOrDefault(x => x.Id == assignedUserId);
            assignedUserName = user?.Email ?? user?.UserName;
        }

        return new SlaCaseItemViewModel
        {
            Id = id,
            CaseNo = caseNo,
            ProductName = productName,
            EventTerm = eventTerm,
            IsSerious = isSerious,
            ReceiptDate = receiptDate,
            DueDate = dueDate,
            Status = status,
            CurrentAssignedRole = assignedRole,
            CurrentAssignedUserName = assignedUserName
        };
    }
    private sealed class UserLookupItem
    {
        public string Id { get; set; } = string.Empty;

        public string? UserName { get; set; }

        public string? Email { get; set; }
    }
}
