using System.Security.Claims;

namespace InnoPV.Web.Services.Security;

public interface IRolePermissionMatrixService
{
    bool CanPerform(ClaimsPrincipal user, string action);

    IReadOnlyCollection<string> GetAllowedRoles(string action);
}
