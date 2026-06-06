namespace InnoPV.Web.Models.CaseHistory;

public class AuditTrailListItemViewModel
{
    public long Id { get; set; }

    public long? PvCaseId { get; set; }

    public string? CaseNo { get; set; }

    public string EntityName { get; set; } = string.Empty;

    public long? EntityId { get; set; }

    public string ActionType { get; set; } = string.Empty;

    public string? FieldName { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public string? Remarks { get; set; }

    public string? PerformedByUserName { get; set; }

    public DateTime PerformedOnUtc { get; set; }
}