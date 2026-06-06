namespace InnoPV.Web.Models.UserManagement;

public class UserListItemViewModel
{
    public string UserId { get; set; } = string.Empty;

    public string? FullName { get; set; }

    public string Email { get; set; } = string.Empty;

    public string? MobileNo { get; set; }

    public string? Designation { get; set; }

    public string? Department { get; set; }

    public string Role { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public bool MustChangePassword { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }
}