using InnoPV.Domain.Entities;
using InnoPV.Domain.Enums;
using InnoPV.Domain.Identity;
using System.Security.Claims;

namespace InnoPV.Web.Services.Security;

public class CaseSecurityService : ICaseSecurityService
{
    private readonly IRolePermissionMatrixService _rolePermissionMatrixService;

    public CaseSecurityService(IRolePermissionMatrixService rolePermissionMatrixService)
    {
        _rolePermissionMatrixService = rolePermissionMatrixService;
    }

    public bool CanPerformAction(ClaimsPrincipal user, string action)
    {
        return _rolePermissionMatrixService.CanPerform(user, action);
    }

    public bool IsCaseReadOnly(PvCase pvCase)
    {
        return pvCase.Status == PvCaseStatus.CaseClosed
               || pvCase.Status == PvCaseStatus.CaseFinalized
               || pvCase.Status == PvCaseStatus.Submitted
               || pvCase.Status == PvCaseStatus.MarkedAsDuplicate
               || pvCase.Status == PvCaseStatus.MarkedAsInvalid;
    }

    public bool CanViewCase(PvCase pvCase, ClaimsPrincipal user, string? currentUserId)
    {
        if (!CanPerformAction(user, PermissionActions.ViewCase))
        {
            return false;
        }

        if (user.IsInRole(AppRoles.Admin))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(currentUserId)
            && pvCase.CreatedBy == currentUserId)
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(currentUserId)
            && pvCase.CurrentAssignedUserId == currentUserId)
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(pvCase.CurrentAssignedRole)
            && user.IsInRole(pvCase.CurrentAssignedRole))
        {
            return true;
        }

        return false;
    }

    public bool CanEditCase(PvCase pvCase, ClaimsPrincipal user, string? currentUserId)
    {
        if (!CanPerformAction(user, PermissionActions.EditCase))
        {
            return false;
        }

        if (IsCaseReadOnly(pvCase))
        {
            return false;
        }

        if (user.IsInRole(AppRoles.Admin))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(currentUserId)
            && pvCase.CurrentAssignedUserId == currentUserId)
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(pvCase.CurrentAssignedRole)
            && user.IsInRole(pvCase.CurrentAssignedRole)
            && string.IsNullOrWhiteSpace(pvCase.CurrentAssignedUserId))
        {
            return true;
        }

        if (user.IsInRole(AppRoles.PvAssociate)
            && pvCase.CreatedBy == currentUserId
            && IsPvAssociateEditableStatus(pvCase.Status))
        {
            return true;
        }

        return false;
    }

    public bool CanProcessWorkflow(PvCase pvCase, ClaimsPrincipal user, string? currentUserId)
    {
        if (!CanPerformAction(user, PermissionActions.ProcessWorkflow))
        {
            return false;
        }

        if (IsCaseReadOnly(pvCase))
        {
            return false;
        }

        if (user.IsInRole(AppRoles.Admin))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(currentUserId)
            && pvCase.CurrentAssignedUserId == currentUserId)
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(pvCase.CurrentAssignedRole)
            && user.IsInRole(pvCase.CurrentAssignedRole)
            && string.IsNullOrWhiteSpace(pvCase.CurrentAssignedUserId))
        {
            return true;
        }

        return false;
    }

    public bool CanUploadAttachment(PvCase pvCase, ClaimsPrincipal user, string? currentUserId)
    {
        if (!CanPerformAction(user, PermissionActions.UploadAttachment))
        {
            return false;
        }

        return CanEditCase(pvCase, user, currentUserId);
    }

    private static bool IsPvAssociateEditableStatus(PvCaseStatus status)
    {
        return status == PvCaseStatus.Draft
               || status == PvCaseStatus.DataEntryInProgress
               || status == PvCaseStatus.ValidityPending
               || status == PvCaseStatus.InvalidFollowUpRequired
               || status == PvCaseStatus.DuplicateCheckPending
               || status == PvCaseStatus.PvAssociateChecklistPending
               || status == PvCaseStatus.ReturnedByPvManager
               || status == PvCaseStatus.AdditionalInformationRequired
               || status == PvCaseStatus.Reopened;
    }
}