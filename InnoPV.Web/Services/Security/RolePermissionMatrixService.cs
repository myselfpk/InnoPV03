using InnoPV.Domain.Identity;
using System.Security.Claims;

namespace InnoPV.Web.Services.Security;

public class RolePermissionMatrixService : IRolePermissionMatrixService
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> Matrix =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            [PermissionActions.AdminOnly] = new[] { AppRoles.Admin },
            [PermissionActions.AdminOrPvManager] = new[] { AppRoles.Admin, AppRoles.PvManager },
            [PermissionActions.AdminOrPvAssociate] = new[] { AppRoles.Admin, AppRoles.PvAssociate },
            [PermissionActions.AdminOrPvManagerOrMedicalReviewer] = new[] { AppRoles.Admin, AppRoles.PvManager, AppRoles.MedicalReviewer },
            [PermissionActions.AdminOrPvAssociateOrPvManager] = new[] { AppRoles.Admin, AppRoles.PvAssociate, AppRoles.PvManager },
            [PermissionActions.AuthenticatedPvUser] = new[] { AppRoles.Admin, AppRoles.PvAssociate, AppRoles.PvManager, AppRoles.MedicalReviewer },
            [PermissionActions.AdminOrMedicalReviewer] = new[] { AppRoles.Admin, AppRoles.MedicalReviewer },

            [PermissionActions.ViewCase] = new[] { AppRoles.Admin, AppRoles.PvAssociate, AppRoles.PvManager, AppRoles.MedicalReviewer },
            [PermissionActions.EditCase] = new[] { AppRoles.Admin, AppRoles.PvAssociate, AppRoles.PvManager, AppRoles.MedicalReviewer },
            [PermissionActions.ProcessWorkflow] = new[] { AppRoles.Admin, AppRoles.PvAssociate, AppRoles.PvManager, AppRoles.MedicalReviewer },
            [PermissionActions.UploadAttachment] = new[] { AppRoles.Admin, AppRoles.PvAssociate, AppRoles.PvManager, AppRoles.MedicalReviewer },

            [PermissionActions.DuplicateCheckView] = new[] { AppRoles.Admin, AppRoles.PvAssociate, AppRoles.PvManager },
            [PermissionActions.DuplicateDecision] = new[] { AppRoles.Admin, AppRoles.PvAssociate, AppRoles.PvManager },

            [PermissionActions.RegulatorySubmissionView] = new[] { AppRoles.Admin, AppRoles.PvManager, AppRoles.MedicalReviewer },
            [PermissionActions.RegulatorySubmissionCreate] = new[] { AppRoles.Admin, AppRoles.PvManager, AppRoles.MedicalReviewer },
            [PermissionActions.RegulatorySubmissionSubmit] = new[] { AppRoles.Admin, AppRoles.PvManager, AppRoles.MedicalReviewer },
            [PermissionActions.RegulatorySubmissionAcknowledge] = new[] { AppRoles.Admin, AppRoles.PvManager, AppRoles.MedicalReviewer },
            [PermissionActions.RegulatorySubmissionDownload] = new[] { AppRoles.Admin, AppRoles.PvManager, AppRoles.MedicalReviewer },
            [PermissionActions.RegulatorySubmissionValidate] = new[] { AppRoles.Admin, AppRoles.PvManager, AppRoles.MedicalReviewer },
            [PermissionActions.RegulatorySubmissionExportValidation] = new[] { AppRoles.Admin, AppRoles.PvManager, AppRoles.MedicalReviewer }
        };

    public bool CanPerform(ClaimsPrincipal user, string action)
    {
        if (!Matrix.TryGetValue(action, out var roles) || roles.Count == 0)
        {
            return false;
        }

        return roles.Any(user.IsInRole);
    }

    public IReadOnlyCollection<string> GetAllowedRoles(string action)
    {
        if (!Matrix.TryGetValue(action, out var roles))
        {
            return Array.Empty<string>();
        }

        return roles;
    }
}
