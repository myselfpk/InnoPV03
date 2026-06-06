using InnoPV.Domain.Enums;
using InnoPV.Domain.Identity;
using InnoPV.Infrastructure.Data;
using InnoPV.Web.Models.CaseInbox;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InnoPV.Web.Controllers;

[Authorize(Policy = AuthorizationPolicies.AuthenticatedPvUser)]
public class CaseInboxController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CaseInboxController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var currentUserId = _userManager.GetUserId(User);
        var role = GetCurrentUserRole();

        var query = _context.PvCases
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        if (role == AppRoles.PvAssociate)
        {
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
                PvCaseStatus.Reopened
            };

            query = query.Where(x =>
                x.CurrentAssignedRole == AppRoles.PvAssociate ||
                (x.CreatedBy == currentUserId && associateStatuses.Contains(x.Status)));
        }
        else if (role == AppRoles.PvManager)
        {
            var managerStatuses = new[]
            {
                PvCaseStatus.SubmittedToPvManager,
                PvCaseStatus.PvManagerReviewPending,
                PvCaseStatus.PvManagerChecklistPending,
                PvCaseStatus.ResubmittedToPvManager,
                PvCaseStatus.ReturnedByMedicalReviewer
            };

            query = query.Where(x =>
                x.CurrentAssignedRole == AppRoles.PvManager ||
                managerStatuses.Contains(x.Status));
        }
        else if (role == AppRoles.MedicalReviewer)
        {
            var medicalReviewerStatuses = new[]
            {
                PvCaseStatus.ForwardedToMedicalReviewer,
                PvCaseStatus.MedicalReviewPending,
                PvCaseStatus.MedicalReviewerChecklistPending,
                PvCaseStatus.MedicallyApproved
            };

            query = query.Where(x =>
                x.CurrentAssignedRole == AppRoles.MedicalReviewer ||
                medicalReviewerStatuses.Contains(x.Status));
        }

        var cases = await query
            .OrderByDescending(x => x.CaseNo)
            .ThenBy(x => x.DueDate)
            .ThenByDescending(x => x.CreatedOnUtc)
            .Select(x => new CaseInboxItemViewModel
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

        var model = new CaseInboxViewModel
        {
            UserRole = role,
            PageTitle = GetPageTitle(role),
            TotalCount = cases.Count,
            OpenCount = cases.Count(x => x.Status != PvCaseStatus.CaseClosed),
            ClosedCount = cases.Count(x => x.Status == PvCaseStatus.CaseClosed),
            OverdueCount = cases.Count(x => x.IsOverdue),
            SeriousCount = cases.Count(x => x.IsSerious),
            ReturnedCount = cases.Count(x =>
                x.Status == PvCaseStatus.ReturnedByPvManager ||
                x.Status == PvCaseStatus.ReturnedByMedicalReviewer),
            Cases = cases
        };

        return View(model);
    }

    private string GetCurrentUserRole()
    {
        if (User.IsInRole(AppRoles.Admin))
        {
            return AppRoles.Admin;
        }

        if (User.IsInRole(AppRoles.MedicalReviewer))
        {
            return AppRoles.MedicalReviewer;
        }

        if (User.IsInRole(AppRoles.PvManager))
        {
            return AppRoles.PvManager;
        }

        return AppRoles.PvAssociate;
    }

    private static string GetPageTitle(string role)
    {
        if (role == AppRoles.Admin)
        {
            return "Admin Case Dashboard";
        }

        if (role == AppRoles.PvAssociate)
        {
            return "PV Associate Case Inbox";
        }

        if (role == AppRoles.PvManager)
        {
            return "PV Manager Review Inbox";
        }

        if (role == AppRoles.MedicalReviewer)
        {
            return "Medical Reviewer Inbox";
        }

        return "Case Inbox";
    }
}
