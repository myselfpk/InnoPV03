using System.ComponentModel.DataAnnotations;

namespace InnoPV.Web.Models.PvCaseEntry;

public class CasePatientDetailViewModel
{
    public long Id { get; set; }

    public long PvCaseId { get; set; }

    public string? CaseNo { get; set; }
    public bool IsReadOnly { get; set; }

    [StringLength(20)]
    public string? PatientInitials { get; set; }

    [StringLength(100)]
    public string? PatientIdentifier { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }

    public int? Age { get; set; }

    [StringLength(20)]
    public string? AgeUnit { get; set; }

    [StringLength(30)]
    public string? Gender { get; set; }

    public decimal? WeightKg { get; set; }

    public decimal? HeightCm { get; set; }

    public bool IsPregnant { get; set; }

    [StringLength(1000)]
    public string? PregnancyRemarks { get; set; }

    [StringLength(2000)]
    public string? RelevantMedicalHistory { get; set; }

    [StringLength(1000)]
    public string? AllergyHistory { get; set; }

    [StringLength(2000)]
    public string? OtherPatientInformation { get; set; }
}