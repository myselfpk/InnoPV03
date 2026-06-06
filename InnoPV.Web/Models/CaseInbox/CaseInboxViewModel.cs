namespace InnoPV.Web.Models.CaseInbox;

public class CaseInboxViewModel
{
    public string UserRole { get; set; } = string.Empty;

    public string PageTitle { get; set; } = "Case Inbox";

    public int TotalCount { get; set; }

    public int OpenCount { get; set; }

    public int ClosedCount { get; set; }

    public int OverdueCount { get; set; }

    public int SeriousCount { get; set; }

    public int ReturnedCount { get; set; }

    public List<CaseInboxItemViewModel> Cases { get; set; } = new();
}