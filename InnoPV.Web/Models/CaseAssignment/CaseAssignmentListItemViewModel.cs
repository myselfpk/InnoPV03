using InnoPV.Domain.Enums;

namespace InnoPV.Web.Models.CaseAssignment;

public class CaseAssignmentListItemViewModel
{
    public long Id { get; set; }

    public string CaseNo { get; set; } = string.Empty;

    public string CaseSource { get; set; } = string.Empty;

    public DateTime ReceiptDate { get; set; }

    public string? InitialReporterName { get; set; }

    public string? InitialPatientIdentifier { get; set; }

    public string? InitialProductName { get; set; }

    public string? InitialEventTerm { get; set; }

    public bool IsSerious { get; set; }

    public DateTime? DueDate { get; set; }

    public PvCaseStatus Status { get; set; }

    public string? CurrentAssignedRole { get; set; }

    public string? CurrentAssignedUserId { get; set; }

    public string? CurrentAssignedUserName { get; set; }

    public DateTime CreatedOnUtc { get; set; }
}