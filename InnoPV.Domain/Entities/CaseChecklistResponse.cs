using InnoPV.Domain.Common;

namespace InnoPV.Domain.Entities;

public class CaseChecklistResponse : BaseEntity
{
    public long PvCaseId { get; set; }
    public PvCase PvCase { get; set; } = null!;

    public long ChecklistItemId { get; set; }
    public ChecklistItem ChecklistItem { get; set; } = null!;

    public string RoleName { get; set; } = string.Empty;

    public bool IsChecked { get; set; }
    public string? Remarks { get; set; }

    public string CheckedByUserId { get; set; } = string.Empty;
    public DateTime? CheckedOnUtc { get; set; }
}