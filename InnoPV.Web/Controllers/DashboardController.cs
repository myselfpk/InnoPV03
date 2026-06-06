using InnoPV.Domain.Enums;
using InnoPV.Domain.Identity;
using InnoPV.Infrastructure.Data;
using InnoPV.Web.Models.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InnoPV.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        if (User.IsInRole(AppRoles.Admin))
        {
            return RedirectToAction(nameof(AdminDashboard));
        }

        if (User.IsInRole(AppRoles.PvAssociate))
        {
            return RedirectToAction(nameof(PvAssociateDashboard));
        }

        if (User.IsInRole(AppRoles.PvManager))
        {
            return RedirectToAction(nameof(PvManagerDashboard));
        }

        if (User.IsInRole(AppRoles.MedicalReviewer))
        {
            return RedirectToAction(nameof(MedicalReviewerDashboard));
        }

        return View();
    }

    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> AdminDashboard()
    {
        var today = DateTime.UtcNow.Date;
        var closedStatuses = new[]
        {
            PvCaseStatus.CaseClosed,
            PvCaseStatus.CaseFinalized,
            PvCaseStatus.Submitted,
            PvCaseStatus.MarkedAsDuplicate,
            PvCaseStatus.MarkedAsInvalid
        };

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
                x.Status,
                x.DueDate,
                x.CreatedOnUtc
            })
            .ToListAsync();

        var totalUsers = await _context.Users.CountAsync();
        var activeUsers = await _context.Users.CountAsync(x => x.IsActive);
        var usersByRole = await _context.Roles
            .AsNoTracking()
            .GroupJoin(
                _context.UserRoles.AsNoTracking(),
                role => role.Id,
                userRole => userRole.RoleId,
                (role, userRoles) => new DashboardChartItemViewModel
                {
                    Label = role.Name ?? "Unknown",
                    Count = userRoles.Count()
                })
            .OrderBy(x => x.Label)
            .ToListAsync();

        var casesByStatus = cases
            .GroupBy(x => x.Status.ToString())
            .OrderBy(x => x.Key)
            .Select(x => new DashboardChartItemViewModel
            {
                Label = x.Key,
                Count = x.Count()
            })
            .ToList();

        var recentCases = cases
            .OrderByDescending(x => x.CreatedOnUtc)
            .Take(8)
            .Select(x => new AdminRecentCaseViewModel
            {
                Id = x.Id,
                CaseNo = x.CaseNo,
                ProductName = x.InitialProductName,
                EventTerm = x.InitialEventTerm,
                Status = x.Status.ToString(),
                CreatedOnUtc = x.CreatedOnUtc,
                DueDate = x.DueDate
            })
            .ToList();

        var recentUsers = await _context.Users
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedOnUtc)
            .Take(8)
            .Select(x => new AdminRecentUserViewModel
            {
                Id = x.Id,
                Name = x.FullName ?? x.UserName ?? x.Email ?? "User",
                Email = x.Email ?? string.Empty,
                IsActive = x.IsActive,
                CreatedOnUtc = x.CreatedOnUtc
            })
            .ToListAsync();

        var model = new AdminDashboardViewModel
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            InactiveUsers = totalUsers - activeUsers,
            TotalCases = cases.Count,
            OpenCases = cases.Count(x => !closedStatuses.Contains(x.Status)),
            ClosedCases = cases.Count(x => closedStatuses.Contains(x.Status)),
            SeriousCases = cases.Count(x => x.IsSerious),
            ValidCases = cases.Count(x => x.IsValidCase),
            InvalidCases = cases.Count(x => !x.IsValidCase),
            OverdueCases = cases.Count(x =>
                x.DueDate.HasValue &&
                x.DueDate.Value.Date < today &&
                !closedStatuses.Contains(x.Status)),
            Products = await _context.ProductMasters.CountAsync(x => !x.IsDeleted),
            Sponsors = await _context.SponsorMasters.CountAsync(x => !x.IsDeleted),
            Studies = await _context.StudyMasters.CountAsync(x => !x.IsDeleted),
            Checklists = await _context.ChecklistMasters.CountAsync(x => !x.IsDeleted),
            Submissions = await _context.CaseRegulatorySubmissions.CountAsync(x => !x.IsDeleted),
            FollowUps = await _context.CaseFollowUps.CountAsync(x => !x.IsDeleted),
            Attachments = await _context.CaseAttachments.CountAsync(x => !x.IsDeleted),
            UsersByRole = usersByRole,
            CasesByStatus = casesByStatus,
            RecentCases = recentCases,
            RecentUsers = recentUsers
        };

        return View(model);
    }

    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.PvAssociate)]
    public IActionResult PvAssociateDashboard()
    {
        return View();
    }

    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.PvManager)]
    public IActionResult PvManagerDashboard()
    {
        return View();
    }

    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.MedicalReviewer)]
    public IActionResult MedicalReviewerDashboard()
    {
        return View();
    }
}
