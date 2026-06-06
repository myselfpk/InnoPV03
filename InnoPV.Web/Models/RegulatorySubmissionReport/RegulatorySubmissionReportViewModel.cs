using InnoPV.Domain.Enums;

namespace InnoPV.Web.Models.RegulatorySubmissionReport;

public class RegulatorySubmissionReportViewModel
{
    public RegulatorySubmissionReportFilterViewModel Filters { get; set; } = new();

    public List<RegulatorySubmissionReportListItemViewModel> Submissions { get; set; } = new();

    public int TotalCount { get; set; }

    public int PendingCount { get; set; }

    public int SubmittedCount { get; set; }

    public int AcknowledgementPendingCount { get; set; }

    public int AcknowledgementReceivedCount { get; set; }

    public int OverdueCount { get; set; }

    public int RejectedCount { get; set; }

    public int CancelledCount { get; set; }
}