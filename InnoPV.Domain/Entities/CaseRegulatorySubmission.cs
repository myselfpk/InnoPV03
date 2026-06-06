using InnoPV.Domain.Common;
using InnoPV.Domain.Enums;

namespace InnoPV.Domain.Entities;

public class CaseRegulatorySubmission : BaseEntity
{
    public long PvCaseId { get; set; }

    public PvCase PvCase { get; set; } = null!;

    public string SubmissionNo { get; set; } = string.Empty;

    public string SubmissionType { get; set; } = string.Empty;

    public string RecipientAuthority { get; set; } = string.Empty;

    public string SubmissionFormat { get; set; } = string.Empty;

    public RegulatorySubmissionStatus SubmissionStatus { get; set; } = RegulatorySubmissionStatus.SubmissionPending;

    public DateTime DueDate { get; set; }

    public DateTime? SubmittedDate { get; set; }

    public DateTime? AcknowledgementReceivedDate { get; set; }

    public string? ReferenceNo { get; set; }

    public string? SubmittedByUserId { get; set; }

    public string? AcknowledgementRemarks { get; set; }

    public string? Remarks { get; set; }

    public string? OriginalFileName { get; set; }

    public string? StoredFileName { get; set; }

    public string? FilePath { get; set; }

    public string? ContentType { get; set; }

    public long? FileSizeBytes { get; set; }
}