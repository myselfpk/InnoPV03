using InnoPV.Domain.Entities;
using InnoPV.Domain.Enums;
using System.Security.Claims;

namespace InnoPV.Web.Services.Security;

public interface ICaseSecurityService
{
    bool CanPerformAction(ClaimsPrincipal user, string action);

    bool IsCaseReadOnly(PvCase pvCase);

    bool CanViewCase(PvCase pvCase, ClaimsPrincipal user, string? currentUserId);

    bool CanEditCase(PvCase pvCase, ClaimsPrincipal user, string? currentUserId);

    bool CanProcessWorkflow(PvCase pvCase, ClaimsPrincipal user, string? currentUserId);

    bool CanUploadAttachment(PvCase pvCase, ClaimsPrincipal user, string? currentUserId);
}