using InnoPV.Domain.Common;

namespace InnoPV.Domain.Entities;

public class AuditTrail : BaseEntity
{
    public long? PvCaseId { get; set; }

    public string? CaseNo { get; set; }

    public string EntityName { get; set; } = string.Empty;

    public long? EntityId { get; set; }

    public string ActionType { get; set; } = string.Empty;

    public string? FieldName { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public string? Remarks { get; set; }

    public string? PerformedByUserId { get; set; }

    public string? PerformedByUserName { get; set; }

    public DateTime PerformedOnUtc { get; set; } = DateTime.UtcNow;
}