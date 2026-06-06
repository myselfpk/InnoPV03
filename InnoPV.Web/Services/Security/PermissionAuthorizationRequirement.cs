using Microsoft.AspNetCore.Authorization;

namespace InnoPV.Web.Services.Security;

public sealed class PermissionAuthorizationRequirement : IAuthorizationRequirement
{
    public PermissionAuthorizationRequirement(string action)
    {
        Action = action;
    }

    public string Action { get; }
}
