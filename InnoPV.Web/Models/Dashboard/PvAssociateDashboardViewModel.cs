using InnoPV.Web.Models.CaseInbox;

namespace InnoPV.Web.Models.Dashboard;

public class PvAssociateDashboardViewModel
{
    public List<DashboardCountCardViewModel> Cards { get; set; } = new();

    public List<DashboardChartItemViewModel> StatusSummary { get; set; } = new();

    public List<CaseInboxItemViewModel> PendingCases { get; set; } = new();

    public List<CaseInboxItemViewModel> DueSoonCases { get; set; } = new();

    public int TotalCases { get; set; }

    public int DraftCount { get; set; }

    public int DataEntryCount { get; set; }

    public int ValidityPendingCount { get; set; }

    public int DuplicateCheckPendingCount { get; set; }

    public int ReturnedCount { get; set; }

    public int OverdueCount { get; set; }

    public int DueWithin7DaysCount { get; set; }
}
