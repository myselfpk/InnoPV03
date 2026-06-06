using InnoPV.Domain.Enums;

namespace InnoPV.Web.Services.Workflow;

public interface ICaseWorkflowTransitionService
{
    WorkflowTransitionValidationResult ValidateSubmit(string roleName, PvCaseStatus currentStatus);
    WorkflowTransitionValidationResult ValidateReturn(string roleName, PvCaseStatus currentStatus);
}
