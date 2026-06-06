namespace InnoPV.Web.Models.Shared;

public class PvCaseActionButtonsViewModel
{
    public long PvCaseId { get; set; }

    public bool ShowView { get; set; } = true;

    public bool ShowEntry { get; set; } = true;

    public bool ShowEdit { get; set; } = true;

    public bool ShowChecklist { get; set; } = true;

    public bool ShowHistory { get; set; } = true;

    public bool ShowDuplicate { get; set; } = true;

    public bool ShowFollowUp { get; set; } = true;

    public bool ShowSubmission { get; set; } = true;

    public bool ShowAssign { get; set; } = true;

    public bool ShowClosure { get; set; } = false;

    public bool ShowNarrative { get; set; } = true;
}