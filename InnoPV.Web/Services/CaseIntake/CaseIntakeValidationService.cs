using InnoPV.Domain.Entities;
using InnoPV.Web.Models.PvCase;

namespace InnoPV.Web.Services.CaseIntake;

public sealed class CaseIntakeValidationService : ICaseIntakeValidationService
{
    public CaseIntakeValidationResult Validate(PvCaseIntakeViewModel model)
    {
        var issues = new List<CaseIntakeValidationIssue>();

        if (string.IsNullOrWhiteSpace(model.CaseSource))
        {
            issues.Add(new CaseIntakeValidationIssue
            {
                FieldName = nameof(model.CaseSource),
                Message = "Case source is required."
            });
        }

        if (model.ReceiptDate.Date > DateTime.Today)
        {
            issues.Add(new CaseIntakeValidationIssue
            {
                FieldName = nameof(model.ReceiptDate),
                Message = "Receipt date cannot be a future date."
            });
        }

        if (model.IsReporterIdentifiable && string.IsNullOrWhiteSpace(model.InitialReporterName))
        {
            issues.Add(new CaseIntakeValidationIssue
            {
                FieldName = nameof(model.InitialReporterName),
                Message = "Reporter name is required when 'Identifiable Reporter' is selected."
            });
        }

        if (model.IsPatientIdentifiable && string.IsNullOrWhiteSpace(model.InitialPatientIdentifier))
        {
            issues.Add(new CaseIntakeValidationIssue
            {
                FieldName = nameof(model.InitialPatientIdentifier),
                Message = "Patient identifier is required when 'Identifiable Patient' is selected."
            });
        }

        if (model.IsSuspectProductAvailable && string.IsNullOrWhiteSpace(model.InitialProductName))
        {
            issues.Add(new CaseIntakeValidationIssue
            {
                FieldName = nameof(model.InitialProductName),
                Message = "Suspect product is required when 'Suspect Product Available' is selected."
            });
        }

        if (model.IsAdverseEventAvailable && string.IsNullOrWhiteSpace(model.InitialEventTerm))
        {
            issues.Add(new CaseIntakeValidationIssue
            {
                FieldName = nameof(model.InitialEventTerm),
                Message = "Adverse event is required when 'Adverse Event Available' is selected."
            });
        }

        return new CaseIntakeValidationResult
        {
            CompletenessScore = CalculateCompletenessScore(model),
            BlockingIssues = issues
        };
    }

    public int CalculateCompletenessScore(PvCase pvCase)
    {
        if (pvCase == null)
        {
            return 0;
        }

        return CalculateCompletenessScore(
            pvCase.CaseSource,
            pvCase.InitialReporterName,
            pvCase.InitialPatientIdentifier,
            pvCase.InitialProductName,
            pvCase.InitialEventTerm,
            pvCase.Narrative,
            pvCase.IsPatientIdentifiable,
            pvCase.IsReporterIdentifiable,
            pvCase.IsSuspectProductAvailable,
            pvCase.IsAdverseEventAvailable);
    }

    private static int CalculateCompletenessScore(PvCaseIntakeViewModel model)
    {
        return CalculateCompletenessScore(
            model.CaseSource,
            model.InitialReporterName,
            model.InitialPatientIdentifier,
            model.InitialProductName,
            model.InitialEventTerm,
            model.Narrative,
            model.IsPatientIdentifiable,
            model.IsReporterIdentifiable,
            model.IsSuspectProductAvailable,
            model.IsAdverseEventAvailable);
    }

    private static int CalculateCompletenessScore(
        string? caseSource,
        string? initialReporterName,
        string? initialPatientIdentifier,
        string? initialProductName,
        string? initialEventTerm,
        string? narrative,
        bool isPatientIdentifiable,
        bool isReporterIdentifiable,
        bool isSuspectProductAvailable,
        bool isAdverseEventAvailable)
    {
        var score = 0;

        if (!string.IsNullOrWhiteSpace(caseSource))
        {
            score += 10;
        }

        if (isPatientIdentifiable)
        {
            score += 15;
        }

        if (isReporterIdentifiable)
        {
            score += 15;
        }

        if (isSuspectProductAvailable)
        {
            score += 15;
        }

        if (isAdverseEventAvailable)
        {
            score += 15;
        }

        if (!string.IsNullOrWhiteSpace(initialReporterName))
        {
            score += 8;
        }

        if (!string.IsNullOrWhiteSpace(initialPatientIdentifier))
        {
            score += 8;
        }

        if (!string.IsNullOrWhiteSpace(initialProductName))
        {
            score += 7;
        }

        if (!string.IsNullOrWhiteSpace(initialEventTerm))
        {
            score += 7;
        }

        if (!string.IsNullOrWhiteSpace(narrative))
        {
            score += 10;
        }

        return Math.Min(score, 100);
    }
}
