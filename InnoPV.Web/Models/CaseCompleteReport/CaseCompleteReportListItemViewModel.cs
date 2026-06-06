using InnoPV.Domain.Enums;

namespace InnoPV.Web.Models.CaseCompleteReport;

public class CaseCompleteReportListItemViewModel
{
    public long Id { get; set; }

    public string CaseNo { get; set; } = string.Empty;

    public string CaseSource { get; set; } = string.Empty;

    public DateTime ReceiptDate { get; set; }

    public PvCaseStatus Status { get; set; }

    public bool IsValidCase { get; set; }

    public bool IsSerious { get; set; }

    public DateTime? DueDate { get; set; }

    public string? InitialReporterName { get; set; }

    public string? InitialPatientIdentifier { get; set; }

    public string? InitialProductName { get; set; }

    public string? InitialEventTerm { get; set; }

    public string? CurrentAssignedRole { get; set; }

    public DateTime CreatedOnUtc { get; set; }
}