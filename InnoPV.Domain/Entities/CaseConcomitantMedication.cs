using InnoPV.Domain.Common;

namespace InnoPV.Domain.Entities;

public class CaseConcomitantMedication : BaseEntity
{
    public long PvCaseId { get; set; }
    public PvCase PvCase { get; set; } = null!;

    public string MedicationName { get; set; } = string.Empty;

    public string? Dose { get; set; }
    public string? DoseUnit { get; set; }
    public string? Route { get; set; }
    public string? Frequency { get; set; }

    public DateTime? TherapyStartDate { get; set; }
    public DateTime? TherapyStopDate { get; set; }

    public string? Indication { get; set; }

    public bool IsMedicationForEventTreatment { get; set; }

    public string? Remarks { get; set; }
}