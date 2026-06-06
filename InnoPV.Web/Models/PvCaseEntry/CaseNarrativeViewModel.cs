namespace InnoPV.Web.Models.PvCaseEntry;

public class CaseNarrativeViewModel
{
    public long PvCaseId { get; set; }

    public string? CaseNo { get; set; }

    public string? InitialNarrative { get; set; }

    public string? CaseNarrativeHtml { get; set; }

    public bool IsReadOnly { get; set; }
}