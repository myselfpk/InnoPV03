namespace InnoPV.Web.Models.Dashboard;

public class PvDashboardViewModel
{
    public List<DashboardCountCardViewModel> Cards { get; set; } = new();

    public List<DashboardChartItemViewModel> StatusChart { get; set; } = new();

    public List<DashboardChartItemViewModel> RoleChart { get; set; } = new();

    public List<DashboardChartItemViewModel> MonthlyTrendChart { get; set; } = new();

    public List<SlaCaseItemViewModel> OverdueCases { get; set; } = new();

    public List<SlaCaseItemViewModel> DueSoonCases { get; set; } = new();

    public int SeriousOpenCount { get; set; }

    public int NonSeriousOpenCount { get; set; }

    public int DueWithin7DaysCount { get; set; }

    public int OverdueCount { get; set; }
}