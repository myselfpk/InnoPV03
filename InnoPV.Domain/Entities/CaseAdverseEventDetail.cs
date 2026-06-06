using InnoPV.Domain.Common;

namespace InnoPV.Domain.Entities;

public class CaseAdverseEventDetail : BaseEntity
{
    public long PvCaseId { get; set; }
    public PvCase PvCase { get; set; } = null!;

    public string EventTerm { get; set; } = string.Empty;
    public string? EventDescription { get; set; }

    public DateTime? EventStartDate { get; set; }
    public DateTime? EventStopDate { get; set; }

    public bool IsSerious { get; set; }
    public string? SeriousnessCriteria { get; set; }

    public string? Severity { get; set; }
    public string? Outcome { get; set; }

    public DateTime? DeathDate { get; set; }
    public string? CauseOfDeath { get; set; }

    public bool WasHospitalized { get; set; }
    public DateTime? HospitalizationDate { get; set; }
    public DateTime? DischargeDate { get; set; }

    public string? TreatmentGivenForEvent { get; set; }
    public string? EventRemarks { get; set; }
}