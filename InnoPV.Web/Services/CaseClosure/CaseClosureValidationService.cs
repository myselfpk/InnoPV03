using InnoPV.Domain.Enums;
using InnoPV.Domain.Identity;
using InnoPV.Infrastructure.Data;
using InnoPV.Web.Models.ClosureValidation;
using Microsoft.EntityFrameworkCore;

namespace InnoPV.Web.Services.CaseClosure;

public class CaseClosureValidationService : ICaseClosureValidationService
{
    private readonly ApplicationDbContext _context;

    public CaseClosureValidationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CaseClosureValidationViewModel> ValidateCaseForClosureAsync(long caseId)
    {
        var pvCase = await _context.PvCases
            .FirstOrDefaultAsync(x => x.Id == caseId && !x.IsDeleted);

        if (pvCase == null)
        {
            return new CaseClosureValidationViewModel
            {
                PvCaseId = caseId,
                CaseNo = "N/A",
                RequiredChecks =
                {
                    Fail("Case", "Case exists", "Case record was not found.")
                }
            };
        }

        var model = new CaseClosureValidationViewModel
        {
            PvCaseId = pvCase.Id,
            CaseNo = pvCase.CaseNo,
            Status = pvCase.Status
        };

        // 1. Case validity
        model.RequiredChecks.Add(
            pvCase.IsValidCase
                ? Pass("Case Validity", "Valid ICSR criteria", "Case is marked as valid.")
                : Fail("Case Validity", "Valid ICSR criteria", "Case is not valid. Patient, reporter, suspect product and adverse event must be available."));

        // 2. Patient details
        var patient = await _context.CasePatientDetails
            .FirstOrDefaultAsync(x => x.PvCaseId == caseId && !x.IsDeleted);

        var patientIdentifiable =
            patient != null &&
            (
                !string.IsNullOrWhiteSpace(patient.PatientIdentifier) ||
                !string.IsNullOrWhiteSpace(patient.PatientInitials) ||
                patient.Age.HasValue ||
                patient.DateOfBirth.HasValue
            );

        model.RequiredChecks.Add(
            patientIdentifiable
                ? Pass("Patient Details", "Patient identifiable information", "Patient identifiable information is available.")
                : Fail("Patient Details", "Patient identifiable information", "Patient identifiable information is missing."));

        // 3. Reporter details
        var reporter = await _context.CaseReporterDetails
            .FirstOrDefaultAsync(x => x.PvCaseId == caseId && !x.IsDeleted);

        model.RequiredChecks.Add(
            reporter != null && !string.IsNullOrWhiteSpace(reporter.ReporterName)
                ? Pass("Reporter Details", "Reporter identifiable information", "Reporter name is available.")
                : Fail("Reporter Details", "Reporter identifiable information", "Reporter name is missing."));

        // 4. Adverse event details
        var adverseEvent = await _context.CaseAdverseEventDetails
            .FirstOrDefaultAsync(x => x.PvCaseId == caseId && !x.IsDeleted);

        model.RequiredChecks.Add(
            adverseEvent != null && !string.IsNullOrWhiteSpace(adverseEvent.EventTerm)
                ? Pass("Adverse Event", "Adverse event term", "Adverse event term is available.")
                : Fail("Adverse Event", "Adverse event term", "Adverse event term is missing."));

        if (pvCase.IsSerious)
        {
            model.RequiredChecks.Add(
                adverseEvent != null && !string.IsNullOrWhiteSpace(adverseEvent.SeriousnessCriteria)
                    ? Pass("Adverse Event", "Seriousness criteria", "Seriousness criteria is available.")
                    : Fail("Adverse Event", "Seriousness criteria", "Seriousness criteria is required for serious case."));
        }

        // 5. Suspect product details
        var suspectProduct = await _context.CaseSuspectProductDetails
            .FirstOrDefaultAsync(x => x.PvCaseId == caseId && !x.IsDeleted);

        model.RequiredChecks.Add(
            suspectProduct != null && !string.IsNullOrWhiteSpace(suspectProduct.ProductName)
                ? Pass("Suspect Product", "Suspect product information", "Suspect product name is available.")
                : Fail("Suspect Product", "Suspect product information", "Suspect product name is missing."));

        // 6. Due date
        model.RequiredChecks.Add(
            pvCase.DueDate.HasValue
                ? Pass("SLA/TAT", "Due date calculation", "Due date is available.")
                : Fail("SLA/TAT", "Due date calculation", "Due date is missing."));

        // 7. Return status check
        var isReturnedStatus =
            pvCase.Status == PvCaseStatus.ReturnedByPvManager ||
            pvCase.Status == PvCaseStatus.ReturnedByMedicalReviewer;

        model.RequiredChecks.Add(
            !isReturnedStatus
                ? Pass("Workflow", "Return status check", "Case is not currently in returned status.")
                : Fail("Workflow", "Return status check", "Case is currently in returned status and cannot be closed."));

        // 8. Mandatory checklist checks for all workflow roles
        await ValidateMandatoryChecklistAsync(
            model,
            caseId,
            AppRoles.PvAssociate,
            "PV Associate Data Entry");

        await ValidateMandatoryChecklistAsync(
            model,
            caseId,
            AppRoles.PvManager,
            "PV Manager QC Review");

        await ValidateMandatoryChecklistAsync(
            model,
            caseId,
            AppRoles.MedicalReviewer,
            "Medical Reviewer Assessment");

        // 9. Serious case attachment requirement
        var attachmentCount = await _context.CaseAttachments
            .CountAsync(x => x.PvCaseId == caseId && !x.IsDeleted);

        if (pvCase.IsSerious)
        {
            model.RequiredChecks.Add(
                attachmentCount > 0
                    ? Pass("Attachments", "Supporting attachment for serious case", "At least one attachment is uploaded.")
                    : Fail("Attachments", "Supporting attachment for serious case", "At least one supporting attachment is required for serious case."));
        }
        else
        {
            model.WarningChecks.Add(
                attachmentCount > 0
                    ? Pass("Attachments", "Supporting attachment", "Attachment is available.")
                    : Fail("Attachments", "Supporting attachment", "No attachment uploaded. Please verify whether attachment is applicable."));
        }

        // 10. Narrative warning
        model.WarningChecks.Add(
            !string.IsNullOrWhiteSpace(pvCase.Narrative)
                ? Pass("Narrative", "Case narrative", "Case narrative is available.")
                : Fail("Narrative", "Case narrative", "Case narrative is blank. Please verify before closure."));

        return model;
    }

    private async Task ValidateMandatoryChecklistAsync(
        CaseClosureValidationViewModel model,
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
                Fail("Checklist", $"{role} mandatory checklist", $"No mandatory checklist item configured for {role}."));

            return;
        }

        var mandatoryItemIds = mandatoryItems
            .Select(x => x.Id)
            .ToList();

        var checkedItemIds = await _context.CaseChecklistResponses
            .Where(x =>
                x.PvCaseId == caseId &&
                mandatoryItemIds.Contains(x.ChecklistItemId) &&
                x.IsChecked &&
                !x.IsDeleted)
            .Select(x => x.ChecklistItemId)
            .ToListAsync();

        var missingItems = mandatoryItems
            .Where(x => !checkedItemIds.Contains(x.Id))
            .Select(x => x.ItemText)
            .ToList();

        model.RequiredChecks.Add(
            !missingItems.Any()
                ? Pass("Checklist", $"{role} mandatory checklist", $"All mandatory checklist items are completed for {role}.")
                : Fail("Checklist", $"{role} mandatory checklist", $"Mandatory checklist pending for {role}: {string.Join("; ", missingItems)}"));
    }

    private static CaseClosureValidationItemViewModel Pass(
        string section,
        string check,
        string message)
    {
        return new CaseClosureValidationItemViewModel
        {
            SectionName = section,
            CheckDescription = check,
            IsPassed = true,
            Message = message
        };
    }

    private static CaseClosureValidationItemViewModel Fail(
        string section,
        string check,
        string message)
    {
        return new CaseClosureValidationItemViewModel
        {
            SectionName = section,
            CheckDescription = check,
            IsPassed = false,
            Message = message
        };
    }
}