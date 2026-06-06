namespace InnoPV.Web.Services.CaseIntake;

public sealed class CaseIntakeValidationIssue
{
    public string FieldName { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
