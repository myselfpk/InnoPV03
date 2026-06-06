using InnoPV.Domain.Identity;
using InnoPV.Infrastructure.Data;
using InnoPV.Web.Models.CaseHistory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InnoPV.Web.Controllers;

[Authorize(Roles = AppRoles.Admin + "," + AppRoles.PvAssociate + "," + AppRoles.PvManager + "," + AppRoles.MedicalReviewer)]
public class CaseHistoryController : Controller
{
    private readonly ApplicationDbContext _context;

    public CaseHistoryController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(long caseId)
    {
        var pvCase = await _context.PvCases
            .FirstOrDefaultAsync(x => x.Id == caseId && !x.IsDeleted);

        if (pvCase == null)
        {
            TempData["ErrorMessage"] = "Case not found.";
            return RedirectToAction("Index", "PvCase");
        }

        var comments = await _context.CaseComments
            .Where(x => x.PvCaseId == caseId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedOnUtc)
            .Select(x => new CaseCommentHistoryViewModel
            {
                Id = x.Id,
                PvCaseId = x.PvCaseId,
                FromRole = x.FromRole,
                ToRole = x.ToRole,
                CommentType = x.CommentType,
                CommentText = x.CommentText,
                CreatedBy = x.CreatedBy,
                CreatedOnUtc = x.CreatedOnUtc
            })
            .ToListAsync();

        var auditTrails = await _context.AuditTrails
            .Where(x => x.PvCaseId == caseId && !x.IsDeleted)
            .OrderByDescending(x => x.PerformedOnUtc)
            .Select(x => new AuditTrailListItemViewModel
            {
                Id = x.Id,
                PvCaseId = x.PvCaseId,
                CaseNo = x.CaseNo,
                EntityName = x.EntityName,
                EntityId = x.EntityId,
                ActionType = x.ActionType,
                FieldName = x.FieldName,
                OldValue = x.OldValue,
                NewValue = x.NewValue,
                Remarks = x.Remarks,
                PerformedByUserName = x.PerformedByUserName,
                PerformedOnUtc = x.PerformedOnUtc
            })
            .ToListAsync();

        var model = new CaseHistoryViewModel
        {
            PvCaseId = pvCase.Id,
            CaseNo = pvCase.CaseNo,
            CaseSource = pvCase.CaseSource,
            ReceiptDate = pvCase.ReceiptDate,
            Status = pvCase.Status,
            Comments = comments,
            AuditTrails = auditTrails
        };

        return View(model);
    }
}