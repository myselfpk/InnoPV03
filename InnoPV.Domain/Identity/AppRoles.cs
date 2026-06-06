using System;
using System.Collections.Generic;
using System.Text;

namespace InnoPV.Domain.Identity;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string PvAssociate = "PV Associate";
    public const string PvManager = "PV Manager";
    public const string MedicalReviewer = "Medical Reviewer";

    public static readonly string[] AllRoles =
    {
        Admin,
        PvAssociate,
        PvManager,
        MedicalReviewer
    };
}
