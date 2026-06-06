using InnoPV.Domain.Enums;

namespace InnoPV.Web.Models.CaseCompleteReport;

public class CaseCompleteReportViewModel
{
    public long PvCaseId { get; set; }

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

    public string? InitialNarrative { get; set; }

    public string? Narrative { get; set; }

    public PatientReportSection Patient { get; set; } = new();

    public ReporterReportSection Reporter { get; set; } = new();

    public AdverseEventReportSection AdverseEvent { get; set; } = new();

    public SuspectProductReportSection SuspectProduct { get; set; } = new();

    public List<ConcomitantMedicationReportSection> ConcomitantMedications { get; set; } = new();

    public List<LabDetailReportSection> LabDetails { get; set; } = new();
}

public class PatientReportSection
{
    public string? PatientIdentifier { get; set; }

    public string? PatientInitials { get; set; }

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

public class ReporterReportSection
{
    public string? ReporterName { get; set; }

    public string? ReporterType { get; set; }

    public string? Qualification { get; set; }

    public string? OrganizationName { get; set; }

    public string? Department { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Country { get; set; }

    public DateTime? DateOfReport { get; set; }

    public bool ConsentForFollowUp { get; set; }

    public string? ReporterRemarks { get; set; }
}

public class AdverseEventReportSection
{
    public string? EventTerm { get; set; }

    public DateTime? EventStartDate { get; set; }

    public DateTime? EventEndDate { get; set; }

    public string? Outcome { get; set; }

    public bool IsSerious { get; set; }

    public string? SeriousnessCriteria { get; set; }

    public string? EventDescription { get; set; }

    public string? TreatmentGiven { get; set; }

    public string? EventRemarks { get; set; }
}

public class SuspectProductReportSection
{
    public string? ProductName { get; set; }

    public string? GenericName { get; set; }

    public string? BatchNo { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public string? Dose { get; set; }

    public string? DoseUnit { get; set; }

    public string? Route { get; set; }

    public string? Frequency { get; set; }

    public DateTime? TherapyStartDate { get; set; }

    public DateTime? TherapyStopDate { get; set; }

    public string? Indication { get; set; }

    public string? ActionTaken { get; set; }

    public string? Dechallenge { get; set; }

    public string? Rechallenge { get; set; }

    public string? CausalityAssessment { get; set; }

    public string? ProductRemarks { get; set; }
}

public class ConcomitantMedicationReportSection
{
    public string? MedicationName { get; set; }

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

public class LabDetailReportSection
{
    public string? TestName { get; set; }

    public DateTime? TestDate { get; set; }

    public string? ResultValue { get; set; }

    public string? Unit { get; set; }

    public string? NormalRange { get; set; }

    public string? ClinicalSignificance { get; set; }

    public string? Remarks { get; set; }
}