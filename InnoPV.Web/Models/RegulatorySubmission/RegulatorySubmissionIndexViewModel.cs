using InnoPV.Domain.Enums;

namespace InnoPV.Web.Models.RegulatorySubmission;

public class RegulatorySubmissionIndexViewModel
{
    public long PvCaseId { get; set; }

    public string CaseNo { get; set; } = string.Empty;

    public PvCaseStatus CaseStatus { get; set; }

    public string? ProductName { get; set; }

    public string? EventTerm { get; set; }

    public int TotalSubmissions { get; set; }

    public int PendingSubmissions { get; set; }

    public int SubmittedCount { get; set; }

    public int AcknowledgementPendingCount { get; set; }

    public int OverdueCount { get; set; }

    public List<RegulatorySubmissionListItemViewModel> Submissions { get; set; } = new();
}