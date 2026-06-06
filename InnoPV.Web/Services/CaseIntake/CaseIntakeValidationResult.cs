namespace InnoPV.Web.Services.CaseIntake;

public sealed class CaseIntakeValidationResult
{
    public int CompletenessScore { get; init; }
    public IReadOnlyList<CaseIntakeValidationIssue> BlockingIssues { get; init; } = Array.Empty<CaseIntakeValidationIssue>();
}
