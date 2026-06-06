using Microsoft.AspNetCore.Authorization;

namespace InnoPV.Web.Services.Security;

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionAuthorizationRequirement>
{
    private readonly IRolePermissionMatrixService _rolePermissionMatrixService;

    public PermissionAuthorizationHandler(IRolePermissionMatrixService rolePermissionMatrixService)
    {
        _rolePermissionMatrixService = rolePermissionMatrixService;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionAuthorizationRequirement requirement)
    {
        if (_rolePermissionMatrixService.CanPerform(context.User, requirement.Action))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
