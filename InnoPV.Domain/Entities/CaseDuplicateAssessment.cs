using InnoPV.Domain.Common;

namespace InnoPV.Domain.Entities;

public class CaseDuplicateAssessment : BaseEntity
{
    public long PvCaseId { get; set; }

    public PvCase PvCase { get; set; } = null!;

    public long? PotentialDuplicatePvCaseId { get; set; }

    public PvCase? PotentialDuplicatePvCase { get; set; }

    public int MatchingScore { get; set; }

    public string? MatchReasons { get; set; }

    public bool IsConfirmedDuplicate { get; set; }

    public string? DecisionRemarks { get; set; }

    public string? AssessedByUserId { get; set; }

    public DateTime? AssessedOnUtc { get; set; }
}