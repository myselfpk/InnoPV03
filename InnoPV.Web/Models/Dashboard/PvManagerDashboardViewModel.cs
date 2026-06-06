using InnoPV.Web.Models.CaseInbox;

namespace InnoPV.Web.Models.Dashboard;

public class PvManagerDashboardViewModel
{
    public List<DashboardCountCardViewModel> Cards { get; set; } = new();

    public List<DashboardChartItemViewModel> StatusSummary { get; set; } = new();

    public List<CaseInboxItemViewModel> PendingCases { get; set; } = new();

    public List<CaseInboxItemViewModel> DueSoonCases { get; set; } = new();

    public int TotalCases { get; set; }

    public int PendingReviewCount { get; set; }

    public int ChecklistPendingCount { get; set; }

    public int ReturnedByMedicalReviewerCount { get; set; }

    public int OverdueCount { get; set; }

    public int DueWithin7DaysCount { get; set; }
}
