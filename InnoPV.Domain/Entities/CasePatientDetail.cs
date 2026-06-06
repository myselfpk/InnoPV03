using InnoPV.Domain.Common;

namespace InnoPV.Domain.Entities;

public class CasePatientDetail : BaseEntity
{
    public long PvCaseId { get; set; }
    public PvCase PvCase { get; set; } = null!;

    public string? PatientInitials { get; set; }
    public string? PatientIdentifier { get; set; }

    public DateTime? DateOfBirth { get; set; }
    public int? Age { get; set; }
    public string? AgeUnit { get; set; }

    public string? Gender { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? HeightCm { get; set; }

    public bool IsPregnant { get; set; }
    public string? PregnancyRemarks { get; set; }

    public string? RelevantMedicalHistory { get; set; }
    public string? AllergyHistory { get; set; }
    public string? OtherPatientInformation { get; set; }
}