using InnoPV.Domain.Common;

namespace InnoPV.Domain.Entities;

public class ChecklistItem : BaseEntity
{
    public long ChecklistMasterId { get; set; }
    public ChecklistMaster ChecklistMaster { get; set; } = null!;

    public string ItemText { get; set; } = string.Empty;
    public bool IsMandatory { get; set; } = true;
    public int DisplayOrder { get; set; }
}