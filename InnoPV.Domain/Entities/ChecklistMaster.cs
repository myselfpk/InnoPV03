using InnoPV.Domain.Common;

namespace InnoPV.Domain.Entities;

public class ChecklistMaster : BaseEntity
{
    public string ChecklistName { get; set; } = string.Empty;
    public string ApplicableRole { get; set; } = string.Empty;
    public string ApplicableStage { get; set; } = string.Empty;

    public string VersionNo { get; set; } = "1.0";
    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;

    public ICollection<ChecklistItem> Items { get; set; } = new List<ChecklistItem>();
}