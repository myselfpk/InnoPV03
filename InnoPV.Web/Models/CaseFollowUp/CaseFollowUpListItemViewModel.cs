namespace InnoPV.Web.Models.CaseFollowUp;

public class CaseFollowUpListItemViewModel
{
    public long Id { get; set; }

    public long PvCaseId { get; set; }

    public string FollowUpNo { get; set; } = string.Empty;

    public DateTime ReceiptDate { get; set; }

    public string Source { get; set; } = string.Empty;

    public string? ReceivedFrom { get; set; }

    public string Description { get; set; } = string.Empty;

    public string? AdditionalInformation { get; set; }

    public bool IsProcessed { get; set; }

    public string? ProcessedRemarks { get; set; }

    public DateTime? ProcessedOnUtc { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedOnUtc { get; set; }
}