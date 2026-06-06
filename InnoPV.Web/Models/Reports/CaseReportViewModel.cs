namespace InnoPV.Web.Models.Reports;

public class CaseReportViewModel
{
    public CaseReportFilterViewModel Filters { get; set; } = new();

    public List<CaseReportListItemViewModel> Cases { get; set; } = new();

    public int TotalCount { get; set; }

    public int ValidCount { get; set; }

    public int InvalidCount { get; set; }

    public int SeriousCount { get; set; }

    public int NonSeriousCount { get; set; }

    public int ClosedCount { get; set; }

    public int OverdueCount { get; set; }
}