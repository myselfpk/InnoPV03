namespace InnoPV.Web.Models.SlaAlerts;

public class SlaAlertSendResultViewModel
{
    public int T7CaseCount { get; set; }

    public int T3CaseCount { get; set; }

    public int T1CaseCount { get; set; }

    public int DueSoonCaseCount { get; set; }

    public int OverdueCaseCount { get; set; }

    public int EmailRecipientCount { get; set; }

    public List<string> Recipients { get; set; } = new();
}