namespace InnoPV.Web.Models.Admin;

public class UserListItemViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public string? Department { get; set; }
    public bool IsActive { get; set; }
    public string Roles { get; set; } = string.Empty;
}