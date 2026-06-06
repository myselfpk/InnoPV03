using System.ComponentModel.DataAnnotations;

namespace InnoPV.Web.Models.Admin;

public class ChecklistItemViewModel
{
    public long Id { get; set; }

    public long ChecklistMasterId { get; set; }

    public string? ChecklistName { get; set; }

    [Required(ErrorMessage = "Checklist item text is required.")]
    [StringLength(500)]
    public string ItemText { get; set; } = string.Empty;

    public bool IsMandatory { get; set; } = true;

    public int DisplayOrder { get; set; } = 1;

    public bool IsActive { get; set; } = true;
}