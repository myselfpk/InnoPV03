namespace InnoPV.Web.Services.Security;

public static class AuthorizationPolicies
{
    public const string AdminOnly = "perm:admin.only";
    public const string AdminOrPvManager = "perm:admin.or.pvmanager";
    public const string AdminOrPvAssociate = "perm:admin.or.pvassociate";
    public const string AdminOrPvManagerOrMedicalReviewer = "perm:admin.or.pvmanager.or.medicalreviewer";
    public const string AdminOrPvAssociateOrPvManager = "perm:admin.or.pvassociate.or.pvmanager";
    public const string AuthenticatedPvUser = "perm:all.pv.roles";
    public const string AdminOrMedicalReviewer = "perm:admin.or.medicalreviewer";
}
