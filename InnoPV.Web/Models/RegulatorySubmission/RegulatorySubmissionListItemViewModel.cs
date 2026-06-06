using InnoPV.Domain.Enums;

namespace InnoPV.Web.Models.RegulatorySubmission;

public class RegulatorySubmissionListItemViewModel
{
    public long Id { get; set; }

    public long PvCaseId { get; set; }

    public string SubmissionNo { get; set; } = string.Empty;

    public string SubmissionType { get; set; } = string.Empty;

    public string RecipientAuthority { get; set; } = string.Empty;

    public string SubmissionFormat { get; set; } = string.Empty;

    public RegulatorySubmissionStatus SubmissionStatus { get; set; }

    public DateTime DueDate { get; set; }

    public DateTime? SubmittedDate { get; set; }

    public DateTime? AcknowledgementReceivedDate { get; set; }

    public string? ReferenceNo { get; set; }

    public string? Remarks { get; set; }

    public string? OriginalFileName { get; set; }

    public string? FilePath { get; set; }

    public bool IsOverdue
    {
        get
        {
            return DueDate.Date < DateTime.UtcNow.Date &&
                   SubmissionStatus != RegulatorySubmissionStatus.Submitted &&
                   SubmissionStatus != RegulatorySubmissionStatus.AcknowledgementReceived &&
                   SubmissionStatus != RegulatorySubmissionStatus.Cancelled;
        }
    }
}