using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace InnoPV.Web.Models.UserManagement;

public class EditUserViewModel
{
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mobile no is required.")]
    [Phone]
    [StringLength(20)]
    public string MobileNo { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Designation { get; set; }

    [StringLength(100)]
    public string? Department { get; set; }

    [Required(ErrorMessage = "Role is required.")]
    public string Role { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public bool MustChangePassword { get; set; }

    public List<SelectListItem> RoleOptions { get; set; } = new();
}