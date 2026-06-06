using InnoPV.Domain.Enums;
using InnoPV.Domain.Identity;

namespace InnoPV.Web.Services.Workflow;

public sealed class CaseWorkflowTransitionService : ICaseWorkflowTransitionService
{
    private static readonly HashSet<PvCaseStatus> PvAssociateSubmitStatuses =
    [
        PvCaseStatus.DataEntryInProgress,
        PvCaseStatus.ReturnedByPvManager,
        PvCaseStatus.Reopened,
        PvCaseStatus.OnHold
    ];

    private static readonly HashSet<PvCaseStatus> PvManagerSubmitStatuses =
    [
        PvCaseStatus.SubmittedToPvManager,
        PvCaseStatus.PvManagerReviewPending,
        PvCaseStatus.PvManagerChecklistPending,
        PvCaseStatus.ResubmittedToPvManager,
        PvCaseStatus.ReturnedByMedicalReviewer,
        PvCaseStatus.PvManagerReviewInProgress
    ];

    private static readonly HashSet<PvCaseStatus> MedicalReviewerSubmitStatuses =
    [
        PvCaseStatus.ForwardedToMedicalReviewer,
        PvCaseStatus.MedicalReviewPending,
        PvCaseStatus.MedicalReviewerChecklistPending,
        PvCaseStatus.MedicallyApproved,
        PvCaseStatus.SubmittedToMedicalReviewer,
        PvCaseStatus.MedicalReviewInProgress
    ];

    public WorkflowTransitionValidationResult ValidateSubmit(string roleName, PvCaseStatus currentStatus)
    {
        if (roleName == AppRoles.PvAssociate)
        {
            return BuildResult(
                PvAssociateSubmitStatuses.Contains(currentStatus),
                "PV Associate is not allowed to submit from the current status.");
        }

        if (roleName == AppRoles.PvManager)
        {
            return BuildResult(
                PvManagerSubmitStatuses.Contains(currentStatus),
                "PV Manager is not allowed to submit from the current status.");
        }

        if (roleName == AppRoles.MedicalReviewer)
        {
            return BuildResult(
                MedicalReviewerSubmitStatuses.Contains(currentStatus),
                "Medical Reviewer is not allowed to submit from the current status.");
        }

        return BuildResult(false, "Current role is not allowed to submit this case.");
    }

    public WorkflowTransitionValidationResult ValidateReturn(string roleName, PvCaseStatus currentStatus)
    {
        if (roleName == AppRoles.PvManager)
        {
            return BuildResult(
                PvManagerSubmitStatuses.Contains(currentStatus),
                "PV Manager is not allowed to return from the current status.");
        }

        if (roleName == AppRoles.MedicalReviewer)
        {
            return BuildResult(
                MedicalReviewerSubmitStatuses.Contains(currentStatus),
                "Medical Reviewer is not allowed to return from the current status.");
        }

        return BuildResult(false, "Current role is not allowed to return this case.");
    }

    private static WorkflowTransitionValidationResult BuildResult(bool isAllowed, string message)
    {
        return new WorkflowTransitionValidationResult
        {
            IsAllowed = isAllowed,
            Message = isAllowed ? string.Empty : message
        };
    }
}
