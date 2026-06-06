using InnoPV.Domain.Enums;

namespace InnoPV.Web.Models.CaseFollowUp;

public class CaseFollowUpIndexViewModel
{
    public long PvCaseId { get; set; }

    public string CaseNo { get; set; } = string.Empty;

    public PvCaseStatus Status { get; set; }

    public string? ProductName { get; set; }

    public string? EventTerm { get; set; }

    public int TotalFollowUps { get; set; }

    public int PendingFollowUps { get; set; }

    public int ProcessedFollowUps { get; set; }

    public bool CanAddFollowUp { get; set; }

    public List<CaseFollowUpListItemViewModel> FollowUps { get; set; } = new();
}