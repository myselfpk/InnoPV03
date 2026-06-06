using InnoPV.Domain.Common;
using InnoPV.Domain.Enums;

namespace InnoPV.Domain.Entities;

public class PvCase : BaseEntity
{
    public string CaseNo { get; set; } = string.Empty;

    public string CaseSource { get; set; } = string.Empty;
    public DateTime ReceiptDate { get; set; }

    public string? InitialReporterName { get; set; }
    public string? InitialPatientIdentifier { get; set; }
    public string? InitialProductName { get; set; }
    public string? InitialEventTerm { get; set; }

    public bool IsPatientIdentifiable { get; set; }
    public bool IsReporterIdentifiable { get; set; }
    public bool IsSuspectProductAvailable { get; set; }
    public bool IsAdverseEventAvailable { get; set; }

    public bool IsValidCase { get; set; }
    public bool IsSerious { get; set; }
    public bool IsDuplicateSuspected { get; set; }

    public PvCaseStatus Status { get; set; } = PvCaseStatus.Draft;

    public string? CurrentAssignedUserId { get; set; }
    public string? CurrentAssignedRole { get; set; }

    public DateTime? DueDate { get; set; }

    public string? Narrative { get; set; }

    public string? CaseNarrativeHtml { get; set; }
    public string? Remarks { get; set; }

}