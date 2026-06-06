namespace InnoPV.Web.Models.Profile;

public class UserProfileViewModel
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? MobileNo { get; set; }

    public string? Designation { get; set; }

    public string? Department { get; set; }

    public string Role { get; set; } = string.Empty;

    public bool MustChangePassword { get; set; }
}