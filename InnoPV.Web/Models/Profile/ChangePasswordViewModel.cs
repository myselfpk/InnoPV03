using System.ComponentModel.DataAnnotations;

namespace InnoPV.Web.Models.Profile;

public class ChangePasswordViewModel
{
    public bool IsForcedChange { get; set; }

    [Required(ErrorMessage = "Current password is required.")]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required.")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirm password is required.")]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "New password and confirm password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}