namespace InnoPV.Web.Models.CaseCompleteReport;

public class CaseCompleteReportIndexViewModel
{
    public CaseCompleteReportFilterViewModel Filters { get; set; } = new();

    public List<CaseCompleteReportListItemViewModel> Cases { get; set; } = new();

    public int TotalCount { get; set; }

    public int SeriousCount { get; set; }

    public int NonSeriousCount { get; set; }

    public int ValidCount { get; set; }

    public int InvalidCount { get; set; }

    public int ClosedCount { get; set; }
}