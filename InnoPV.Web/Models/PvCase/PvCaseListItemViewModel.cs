using InnoPV.Domain.Enums;

namespace InnoPV.Web.Models.PvCase;

public class PvCaseListItemViewModel
{
    public long Id { get; set; }

    public string CaseNo { get; set; } = string.Empty;

    public string CaseSource { get; set; } = string.Empty;

    public DateTime ReceiptDate { get; set; }

    public string? InitialReporterName { get; set; }

    public string? InitialPatientIdentifier { get; set; }

    public string? InitialProductName { get; set; }

    public string? InitialEventTerm { get; set; }

    public bool IsValidCase { get; set; }

    public bool IsSerious { get; set; }

    public int CompletenessScore { get; set; }

    public DateTime? DueDate { get; set; }

    public PvCaseStatus Status { get; set; }
}