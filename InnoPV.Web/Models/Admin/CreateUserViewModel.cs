using System.ComponentModel.DataAnnotations;

namespace InnoPV.Web.Models.Admin;

public class CreateUserViewModel
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Designation { get; set; }

    [StringLength(100)]
    public string? Department { get; set; }

    [Required(ErrorMessage = "Role is required.")]
    public string RoleName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirm password is required.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Password and confirm password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public List<string> AvailableRoles { get; set; } = new();
}