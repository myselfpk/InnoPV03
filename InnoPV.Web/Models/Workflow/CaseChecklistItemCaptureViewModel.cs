using System.ComponentModel.DataAnnotations;

namespace InnoPV.Web.Models.Workflow;

public class CaseChecklistItemCaptureViewModel
{
    public long ChecklistItemId { get; set; }

    public string ItemText { get; set; } = string.Empty;

    public bool IsMandatory { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsChecked { get; set; }

    [StringLength(1000)]
    public string? Remarks { get; set; }
}