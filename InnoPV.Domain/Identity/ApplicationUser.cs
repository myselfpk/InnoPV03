using Microsoft.AspNetCore.Identity;

namespace InnoPV.Domain.Identity;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
    public string? Designation { get; set; }
    public string? Department { get; set; }

    public bool IsActive { get; set; } = true;

    public bool MustChangePassword { get; set; } = true;

    public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginOnUtc { get; set; }
}