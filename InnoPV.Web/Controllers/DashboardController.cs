using InnoPV.Domain.Enums;
using InnoPV.Domain.Identity;
using InnoPV.Infrastructure.Data;
using InnoPV.Web.Models.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
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

    [Authorize(Policy = AuthorizationPolicies.AdminOrPvAssociate)]
    public async Task<IActionResult> PvAssociateDashboard()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var today = DateTime.UtcNow.Date;
        var next7Days = today.AddDays(7);

        var associateStatuses = new[]
        {
            PvCaseStatus.Draft,
            PvCaseStatus.DataEntryInProgress,
            PvCaseStatus.ValidityPending,
            PvCaseStatus.InvalidFollowUpRequired,
            PvCaseStatus.DuplicateCheckPending,
            PvCaseStatus.PvAssociateChecklistPending,
            PvCaseStatus.ReturnedByPvManager,
            PvCaseStatus.AdditionalInformationRequired,
            PvCaseStatus.Reopened,
            PvCaseStatus.ReturnedToPvAssociate
        };

        var cases = await _context.PvCases
            .AsNoTracking()
            .Where(x =>
                !x.IsDeleted &&
                (x.CurrentAssignedUserId == currentUserId ||
                 (x.CreatedBy == currentUserId && associateStatuses.Contains(x.Status))))
            .OrderBy(x => x.DueDate)
            .ThenByDescending(x => x.CreatedOnUtc)
            .Select(x => new InnoPV.Web.Models.CaseInbox.CaseInboxItemViewModel
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
                CreatedBy = x.CreatedBy,
                CreatedOnUtc = x.CreatedOnUtc
            })
            .ToListAsync();

        var draftCount = cases.Count(x => x.Status == PvCaseStatus.Draft);
        var dataEntryCount = cases.Count(x => x.Status == PvCaseStatus.DataEntryInProgress);
        var validityPendingCount = cases.Count(x =>
            x.Status == PvCaseStatus.ValidityPending ||
            x.Status == PvCaseStatus.InvalidFollowUpRequired);
        var duplicateCheckPendingCount = cases.Count(x =>
            x.Status == PvCaseStatus.DuplicateCheckPending);
        var returnedCount = cases.Count(x =>
            x.Status == PvCaseStatus.ReturnedByPvManager ||
            x.Status == PvCaseStatus.ReturnedToPvAssociate);
        var overdueCount = cases.Count(x =>
            x.DueDate.HasValue &&
            x.DueDate.Value.Date < today);

        var dueSoonCases = cases
            .Where(x =>
                x.DueDate.HasValue &&
                x.DueDate.Value.Date >= today &&
                x.DueDate.Value.Date <= next7Days)
            .OrderBy(x => x.DueDate)
            .ToList();

        var model = new PvAssociateDashboardViewModel
        {
            TotalCases = cases.Count,
            DraftCount = draftCount,
            DataEntryCount = dataEntryCount,
            ValidityPendingCount = validityPendingCount,
            DuplicateCheckPendingCount = duplicateCheckPendingCount,
            ReturnedCount = returnedCount,
            OverdueCount = overdueCount,
            DueWithin7DaysCount = dueSoonCases.Count,
            Cards = new List<DashboardCountCardViewModel>
            {
                new()
                {
                    Title = "Associate Queue",
                    Count = cases.Count,
                    Description = "Cases assigned or created by you"
                },
                new()
                {
                    Title = "Data Entry",
                    Count = draftCount + dataEntryCount,
                    Description = "Draft and in-progress cases"
                },
                new()
                {
                    Title = "Validity Pending",
                    Count = validityPendingCount,
                    Description = "Validity or follow-up action needed"
                },
                new()
                {
                    Title = "Duplicate Check",
                    Count = duplicateCheckPendingCount,
                    Description = "Cases awaiting duplicate assessment"
                }
            },
            StatusSummary = cases
                .GroupBy(x => x.Status.ToString())
                .OrderBy(x => x.Key)
                .Select(x => new DashboardChartItemViewModel
                {
                    Label = x.Key,
                    Count = x.Count()
                })
                .ToList(),
            PendingCases = cases.Take(10).ToList(),
            DueSoonCases = dueSoonCases.Take(10).ToList()
        };

        return View(model);
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOrPvManager)]
    public async Task<IActionResult> PvManagerDashboard()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var today = DateTime.UtcNow.Date;
        var next7Days = today.AddDays(7);

        var managerStatuses = new[]
        {
            PvCaseStatus.SubmittedToPvManager,
            PvCaseStatus.PvManagerReviewPending,
            PvCaseStatus.PvManagerChecklistPending,
            PvCaseStatus.ResubmittedToPvManager,
            PvCaseStatus.ReturnedByMedicalReviewer,
            PvCaseStatus.PvManagerReviewInProgress,
            PvCaseStatus.ReturnedToPvManager
        };

        var cases = await _context.PvCases
            .AsNoTracking()
            .Where(x =>
                !x.IsDeleted &&
                managerStatuses.Contains(x.Status) &&
                (x.CurrentAssignedUserId == currentUserId ||
                 x.CreatedBy == currentUserId))
            .OrderBy(x => x.DueDate)
            .ThenByDescending(x => x.CreatedOnUtc)
            .Select(x => new InnoPV.Web.Models.CaseInbox.CaseInboxItemViewModel
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
                CreatedBy = x.CreatedBy,
                CreatedOnUtc = x.CreatedOnUtc
            })
            .ToListAsync();

        var pendingReviewCount = cases.Count(x =>
            x.Status == PvCaseStatus.SubmittedToPvManager ||
            x.Status == PvCaseStatus.PvManagerReviewPending ||
            x.Status == PvCaseStatus.ResubmittedToPvManager ||
            x.Status == PvCaseStatus.PvManagerReviewInProgress ||
            x.Status == PvCaseStatus.ReturnedToPvManager);

        var checklistPendingCount = cases.Count(x =>
            x.Status == PvCaseStatus.PvManagerChecklistPending);

        var returnedByMedicalReviewerCount = cases.Count(x =>
            x.Status == PvCaseStatus.ReturnedByMedicalReviewer);

        var overdueCount = cases.Count(x =>
            x.DueDate.HasValue &&
            x.DueDate.Value.Date < today);

        var dueSoonCases = cases
            .Where(x =>
                x.DueDate.HasValue &&
                x.DueDate.Value.Date >= today &&
                x.DueDate.Value.Date <= next7Days)
            .OrderBy(x => x.DueDate)
            .ToList();

        var model = new PvManagerDashboardViewModel
        {
            TotalCases = cases.Count,
            PendingReviewCount = pendingReviewCount,
            ChecklistPendingCount = checklistPendingCount,
            ReturnedByMedicalReviewerCount = returnedByMedicalReviewerCount,
            OverdueCount = overdueCount,
            DueWithin7DaysCount = dueSoonCases.Count,
            Cards = new List<DashboardCountCardViewModel>
            {
                new()
                {
                    Title = "Manager Queue",
                    Count = cases.Count,
                    Description = "Cases assigned to you"
                },
                new()
                {
                    Title = "Pending Review",
                    Count = pendingReviewCount,
                    Description = "QC review in progress or pending"
                },
                new()
                {
                    Title = "Checklist Pending",
                    Count = checklistPendingCount,
                    Description = "PV Manager checklist action needed"
                },
                new()
                {
                    Title = "Returned Cases",
                    Count = returnedByMedicalReviewerCount,
                    Description = "Returned by Medical Reviewer"
                }
            },
            StatusSummary = cases
                .GroupBy(x => x.Status.ToString())
                .OrderBy(x => x.Key)
                .Select(x => new DashboardChartItemViewModel
                {
                    Label = x.Key,
                    Count = x.Count()
                })
                .ToList(),
            PendingCases = cases.Take(10).ToList(),
            DueSoonCases = dueSoonCases.Take(10).ToList()
        };

        return View(model);
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOrMedicalReviewer)]
    public async Task<IActionResult> MedicalReviewerDashboard()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var today = DateTime.UtcNow.Date;
        var next7Days = today.AddDays(7);

        var medicalReviewerStatuses = new[]
        {
            PvCaseStatus.ForwardedToMedicalReviewer,
            PvCaseStatus.MedicalReviewPending,
            PvCaseStatus.MedicalReviewerChecklistPending,
            PvCaseStatus.MedicallyApproved,
            PvCaseStatus.SubmittedToMedicalReviewer,
            PvCaseStatus.MedicalReviewInProgress
        };

        var cases = await _context.PvCases
            .AsNoTracking()
            .Where(x =>
                !x.IsDeleted &&
                medicalReviewerStatuses.Contains(x.Status) &&
                (x.CurrentAssignedUserId == currentUserId ||
                 x.CreatedBy == currentUserId))
            .OrderBy(x => x.DueDate)
            .ThenByDescending(x => x.CreatedOnUtc)
            .Select(x => new InnoPV.Web.Models.CaseInbox.CaseInboxItemViewModel
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
                CreatedBy = x.CreatedBy,
                CreatedOnUtc = x.CreatedOnUtc
            })
            .ToListAsync();

        var assessmentPendingCount = cases.Count(x =>
            x.Status == PvCaseStatus.ForwardedToMedicalReviewer ||
            x.Status == PvCaseStatus.MedicalReviewPending ||
            x.Status == PvCaseStatus.SubmittedToMedicalReviewer ||
            x.Status == PvCaseStatus.MedicalReviewInProgress);

        var checklistPendingCount = cases.Count(x =>
            x.Status == PvCaseStatus.MedicalReviewerChecklistPending);

        var approvedCount = cases.Count(x =>
            x.Status == PvCaseStatus.MedicallyApproved);

        var overdueCount = cases.Count(x =>
            x.DueDate.HasValue &&
            x.DueDate.Value.Date < today);

        var dueSoonCases = cases
            .Where(x =>
                x.DueDate.HasValue &&
                x.DueDate.Value.Date >= today &&
                x.DueDate.Value.Date <= next7Days)
            .OrderBy(x => x.DueDate)
            .ToList();

        var model = new MedicalReviewerDashboardViewModel
        {
            TotalCases = cases.Count,
            AssessmentPendingCount = assessmentPendingCount,
            ChecklistPendingCount = checklistPendingCount,
            ApprovedCount = approvedCount,
            OverdueCount = overdueCount,
            DueWithin7DaysCount = dueSoonCases.Count,
            Cards = new List<DashboardCountCardViewModel>
            {
                new()
                {
                    Title = "Reviewer Queue",
                    Count = cases.Count,
                    Description = "Cases assigned to you"
                },
                new()
                {
                    Title = "Assessment Pending",
                    Count = assessmentPendingCount,
                    Description = "Medical assessment required"
                },
                new()
                {
                    Title = "Checklist Pending",
                    Count = checklistPendingCount,
                    Description = "Reviewer checklist action needed"
                },
                new()
                {
                    Title = "Approved",
                    Count = approvedCount,
                    Description = "Cases medically approved"
                }
            },
            StatusSummary = cases
                .GroupBy(x => x.Status.ToString())
                .OrderBy(x => x.Key)
                .Select(x => new DashboardChartItemViewModel
                {
                    Label = x.Key,
                    Count = x.Count()
                })
                .ToList(),
            PendingCases = cases.Take(10).ToList(),
            DueSoonCases = dueSoonCases.Take(10).ToList()
        };

        return View(model);
    }
}
