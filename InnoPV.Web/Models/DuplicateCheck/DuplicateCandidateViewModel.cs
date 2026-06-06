using InnoPV.Domain.Enums;

namespace InnoPV.Web.Models.DuplicateCheck;

public class DuplicateCandidateViewModel
{
    public long Id { get; set; }

    public string CaseNo { get; set; } = string.Empty;

    public string? PatientIdentifier { get; set; }

    public string? ReporterName { get; set; }

    public string? ProductName { get; set; }

    public string? EventTerm { get; set; }

    public DateTime ReceiptDate { get; set; }

    public PvCaseStatus Status { get; set; }

    public int MatchingScore { get; set; }

    public string ConfidenceBand { get; set; } = string.Empty;

    public string MatchReasons { get; set; } = string.Empty;
}