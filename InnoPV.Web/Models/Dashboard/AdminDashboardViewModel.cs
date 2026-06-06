namespace InnoPV.Web.Models.Dashboard;

public class AdminDashboardViewModel
{
    public int TotalUsers { get; set; }

    public int ActiveUsers { get; set; }

    public int InactiveUsers { get; set; }

    public int TotalCases { get; set; }

    public int OpenCases { get; set; }

    public int ClosedCases { get; set; }

    public int OverdueCases { get; set; }

    public int Products { get; set; }

    public int Sponsors { get; set; }

    public int Studies { get; set; }

    public int Checklists { get; set; }

    public int SeriousCases { get; set; }

    public int ValidCases { get; set; }

    public int InvalidCases { get; set; }

    public int Submissions { get; set; }

    public int FollowUps { get; set; }

    public int Attachments { get; set; }

    public List<DashboardChartItemViewModel> UsersByRole { get; set; } = new();

    public List<DashboardChartItemViewModel> CasesByStatus { get; set; } = new();

    public List<AdminRecentCaseViewModel> RecentCases { get; set; } = new();

    public List<AdminRecentUserViewModel> RecentUsers { get; set; } = new();
}

public class AdminRecentCaseViewModel
{
    public long Id { get; set; }

    public string CaseNo { get; set; } = string.Empty;

    public string? ProductName { get; set; }

    public string? EventTerm { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedOnUtc { get; set; }

    public DateTime? DueDate { get; set; }
}

public class AdminRecentUserViewModel
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime CreatedOnUtc { get; set; }
}
