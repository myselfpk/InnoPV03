using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace InnoPV.Web.Models.Admin;

public class ChecklistMasterViewModel
{
    public long Id { get; set; }

    [Required(ErrorMessage = "Checklist name is required.")]
    [StringLength(200)]
    public string ChecklistName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Applicable role is required.")]
    [StringLength(100)]
    public string ApplicableRole { get; set; } = string.Empty;

    [Required(ErrorMessage = "Applicable stage is required.")]
    [StringLength(100)]
    public string ApplicableStage { get; set; } = string.Empty;

    [Required(ErrorMessage = "Version no. is required.")]
    [StringLength(20)]
    public string VersionNo { get; set; } = "1.0";

    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public List<SelectListItem> RoleOptions { get; set; } = new();

    public List<SelectListItem> StageOptions { get; set; } = new();
}