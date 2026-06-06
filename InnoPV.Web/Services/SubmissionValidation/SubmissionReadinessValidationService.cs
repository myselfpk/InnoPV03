using InnoPV.Domain.Entities;
using InnoPV.Domain.Enums;
using InnoPV.Domain.Identity;
using InnoPV.Infrastructure.Data;
using InnoPV.Web.Models.SubmissionValidation;
using Microsoft.EntityFrameworkCore;

namespace InnoPV.Web.Services.SubmissionValidation;

public class SubmissionReadinessValidationService : ISubmissionReadinessValidationService
{
    private readonly ApplicationDbContext _context;

    public SubmissionReadinessValidationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SubmissionReadinessValidationViewModel> ValidateCaseForSubmissionAsync(long caseId)
    {
        var pvCase = await _context.PvCases
            .FirstOrDefaultAsync(x => x.Id == caseId && !x.IsDeleted);

        if (pvCase == null)
        {
            return new SubmissionReadinessValidationViewModel
            {
                PvCaseId = caseId,
                CaseNo = "N/A",
                RequiredChecks =
                {
                    Fail("Case", "Case exists", "Case record was not found.")
                }
            };
        }

        var model = new SubmissionReadinessValidationViewModel
        {
            PvCaseId = pvCase.Id,
            CaseNo = pvCase.CaseNo,
            Status = pvCase.Status,
            GeneratedOnUtc = DateTime.UtcNow
        };

        model.RequiredChecks.Add(
            pvCase.IsValidCase
                ? Pass("Case Validity", "Valid ICSR criteria", "Case is marked as valid.")
                : Fail("Case Validity", "Valid ICSR criteria", "Case is not valid. Complete patient/reporter/product/event validity criteria."));

        model.RequiredChecks.Add(
            pvCase.DueDate.HasValue
                ? Pass("SLA/TAT", "Due date available", "Due date is available for regulatory tracking.")
                : Fail("SLA/TAT", "Due date available", "Due date is missing."));

        var hasPatientDetail = await _context.CasePatientDetails
            .AnyAsync(x => x.PvCaseId == caseId && !x.IsDeleted);

        model.RequiredChecks.Add(
            hasPatientDetail
                ? Pass("Patient", "Patient detail record", "Patient detail record is available.")
                : Fail("Patient", "Patient detail record", "Patient detail record is missing."));

        var hasReporterDetail = await _context.CaseReporterDetails
            .AnyAsync(x => x.PvCaseId == caseId && !x.IsDeleted);

        model.RequiredChecks.Add(
            hasReporterDetail
                ? Pass("Reporter", "Reporter detail record", "Reporter detail record is available.")
                : Fail("Reporter", "Reporter detail record", "Reporter detail record is missing."));

        var hasEventDetail = await _context.CaseAdverseEventDetails
            .AnyAsync(x => x.PvCaseId == caseId && !x.IsDeleted);

        model.RequiredChecks.Add(
            hasEventDetail
                ? Pass("Adverse Event", "Adverse event detail record", "Adverse event detail record is available.")
                : Fail("Adverse Event", "Adverse event detail record", "Adverse event detail record is missing."));

        var hasSuspectProduct = await _context.CaseSuspectProductDetails
            .AnyAsync(x => x.PvCaseId == caseId && !x.IsDeleted);

        model.RequiredChecks.Add(
            hasSuspectProduct
                ? Pass("Suspect Product", "Suspect product detail record", "Suspect product detail record is available.")
                : Fail("Suspect Product", "Suspect product detail record", "Suspect product detail record is missing."));

        model.RequiredChecks.Add(
            pvCase.Status == PvCaseStatus.MedicallyApproved ||
            pvCase.Status == PvCaseStatus.SubmissionPending ||
            pvCase.Status == PvCaseStatus.AcknowledgementPending ||
            pvCase.Status == PvCaseStatus.Submitted
                ? Pass("Workflow", "Medical approval before submission", "Case is in a submission-eligible workflow status.")
                : Fail("Workflow", "Medical approval before submission", "Case is not in medical-approved/submission-eligible status."));

        if (pvCase.IsDuplicateSuspected)
        {
            var hasDuplicateDecision = await _context.CaseDuplicateAssessments
                .AnyAsync(x => x.PvCaseId == caseId && !x.IsDeleted);

            model.RequiredChecks.Add(
                hasDuplicateDecision
                    ? Pass("Duplicate Check", "Duplicate decision record", "Duplicate decision is documented.")
                    : Fail("Duplicate Check", "Duplicate decision record", "Duplicate is suspected but no duplicate decision has been recorded."));
        }

        await ValidateMandatoryChecklistAsync(model, caseId, AppRoles.PvAssociate, "PV Associate Data Entry");
        await ValidateMandatoryChecklistAsync(model, caseId, AppRoles.PvManager, "PV Manager QC Review");
        await ValidateMandatoryChecklistAsync(model, caseId, AppRoles.MedicalReviewer, "Medical Reviewer Assessment");

        var hasNarrative = !string.IsNullOrWhiteSpace(pvCase.Narrative);
        model.WarningChecks.Add(
            hasNarrative
                ? Pass("Narrative", "Case narrative", "Case narrative is available.")
                : Fail("Narrative", "Case narrative", "Case narrative is blank. Review before submission."));

        var hasAttachment = await _context.CaseAttachments
            .AnyAsync(x => x.PvCaseId == caseId && !x.IsDeleted);

        model.WarningChecks.Add(
            hasAttachment
                ? Pass("Attachments", "Supporting document", "At least one supporting attachment is available.")
                : Fail("Attachments", "Supporting document", "No supporting attachment found."));

        return model;
    }

    private async Task ValidateMandatoryChecklistAsync(
        SubmissionReadinessValidationViewModel model,
        long caseId,
        string role,
        string stage)
    {
        var checklistMaster = await _context.ChecklistMasters
            .Where(x =>
                x.ApplicableRole == role &&
                x.ApplicableStage == stage &&
                x.IsActive &&
                !x.IsDeleted)
            .OrderByDescending(x => x.EffectiveFrom)
            .FirstOrDefaultAsync();

        if (checklistMaster == null)
        {
            model.RequiredChecks.Add(
                Fail("Checklist", $"{role} checklist", $"No active checklist found for {role} - {stage}."));
            return;
        }

        var mandatoryItems = await _context.ChecklistItems
            .Where(x =>
                x.ChecklistMasterId == checklistMaster.Id &&
                x.IsMandatory &&
                x.IsActive &&
                !x.IsDeleted)
            .ToListAsync();

        if (!mandatoryItems.Any())
        {
            model.WarningChecks.Add(
                Fail("Checklist", $"{role} mandatory checklist", $"No mandatory checklist configured for {role}."));
            return;
        }

        var mandatoryIds = mandatoryItems.Select(x => x.Id).ToList();

        var checkedIds = await _context.CaseChecklistResponses
            .Where(x =>
                x.PvCaseId == caseId &&
                mandatoryIds.Contains(x.ChecklistItemId) &&
                x.IsChecked &&
                !x.IsDeleted)
            .Select(x => x.ChecklistItemId)
            .ToListAsync();

        var missingItems = mandatoryItems
            .Where(x => !checkedIds.Contains(x.Id))
            .Select(x => x.ItemText)
            .ToList();

        model.RequiredChecks.Add(
            !missingItems.Any()
                ? Pass("Checklist", $"{role} mandatory checklist", $"All mandatory checklist items completed for {role}.")
                : Fail("Checklist", $"{role} mandatory checklist", $"Mandatory checklist pending for {role}: {string.Join("; ", missingItems)}"));
    }

    private static SubmissionReadinessCheckItemViewModel Pass(string section, string check, string message)
    {
        return new SubmissionReadinessCheckItemViewModel
        {
            SectionName = section,
            CheckDescription = check,
            IsPassed = true,
            Message = message
        };
    }

    private static SubmissionReadinessCheckItemViewModel Fail(string section, string check, string message)
    {
        return new SubmissionReadinessCheckItemViewModel
        {
            SectionName = section,
            CheckDescription = check,
            IsPassed = false,
            Message = message
        };
    }
}
