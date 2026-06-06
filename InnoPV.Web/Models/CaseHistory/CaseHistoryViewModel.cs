using InnoPV.Domain.Enums;

namespace InnoPV.Web.Models.CaseHistory;

public class CaseHistoryViewModel
{
    public long PvCaseId { get; set; }

    public string CaseNo { get; set; } = string.Empty;

    public string CaseSource { get; set; } = string.Empty;

    public DateTime ReceiptDate { get; set; }

    public PvCaseStatus Status { get; set; }

    public List<CaseCommentHistoryViewModel> Comments { get; set; } = new();

    public List<AuditTrailListItemViewModel> AuditTrails { get; set; } = new();
}