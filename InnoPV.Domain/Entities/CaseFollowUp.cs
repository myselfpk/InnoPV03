using InnoPV.Domain.Common;

namespace InnoPV.Domain.Entities;

public class CaseFollowUp : BaseEntity
{
    public long PvCaseId { get; set; }

    public PvCase PvCase { get; set; } = null!;

    public string FollowUpNo { get; set; } = string.Empty;

    public DateTime ReceiptDate { get; set; }

    public string Source { get; set; } = string.Empty;

    public string? ReceivedFrom { get; set; }

    public string Description { get; set; } = string.Empty;

    public string? AdditionalInformation { get; set; }

    public bool IsProcessed { get; set; }

    public string? ProcessedRemarks { get; set; }

    public string? ProcessedByUserId { get; set; }

    public DateTime? ProcessedOnUtc { get; set; }
}