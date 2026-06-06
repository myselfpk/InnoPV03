namespace InnoPV.Web.Services.Workflow;

public sealed class WorkflowTransitionValidationResult
{
    public bool IsAllowed { get; init; }
    public string Message { get; init; } = string.Empty;
}
