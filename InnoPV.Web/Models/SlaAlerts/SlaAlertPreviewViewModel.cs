namespace InnoPV.Web.Models.SlaAlerts;

public class SlaAlertPreviewViewModel
{
    public int T7Days { get; set; } = 7;

    public int T3Days { get; set; } = 3;

    public int T1Days { get; set; } = 1;

    public List<SlaAlertCaseItemViewModel> T7Cases { get; set; } = new();

    public List<SlaAlertCaseItemViewModel> T3Cases { get; set; } = new();

    public List<SlaAlertCaseItemViewModel> T1Cases { get; set; } = new();

    public List<SlaAlertCaseItemViewModel> DueSoonCases { get; set; } = new();

    public List<SlaAlertCaseItemViewModel> OverdueCases { get; set; } = new();

    public int T7Count => T7Cases.Count;

    public int T3Count => T3Cases.Count;

    public int T1Count => T1Cases.Count;

    public int DueSoonCount => DueSoonCases.Count;

    public int OverdueCount => OverdueCases.Count;

    public DateTime GeneratedOnUtc { get; set; } = DateTime.UtcNow;
}