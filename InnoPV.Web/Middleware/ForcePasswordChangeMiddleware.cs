using InnoPV.Domain.Identity;
using Microsoft.AspNetCore.Identity;

namespace InnoPV.Web.Middleware;

public class ForcePasswordChangeMiddleware
{
    private readonly RequestDelegate _next;

    public ForcePasswordChangeMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        UserManager<ApplicationUser> userManager)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

            var allowedPaths = new[]
            {
                "/profile/changepassword",
                "/account/logout",
                "/account/login",
                "/account/forgotpassword",
                "/account/forgotpasswordconfirmation",
                "/account/resetpassword",
                "/lib/",
                "/css/",
                "/js/",
                "/favicon"
            };

            var isAllowedPath = allowedPaths.Any(x => path.StartsWith(x));

            if (!isAllowedPath)
            {
                var user = await userManager.GetUserAsync(context.User);

                if (user != null && user.MustChangePassword)
                {
                    context.Response.Redirect("/Profile/ChangePassword?force=true");
                    return;
                }
            }
        }

        await _next(context);
    }
}