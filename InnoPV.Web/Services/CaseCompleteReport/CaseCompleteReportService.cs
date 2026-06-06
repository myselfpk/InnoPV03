using InnoPV.Infrastructure.Data;
using InnoPV.Web.Models.CaseCompleteReport;
using Microsoft.EntityFrameworkCore;

namespace InnoPV.Web.Services.CaseCompleteReport;

public class CaseCompleteReportService : ICaseCompleteReportService
{
    private readonly ApplicationDbContext _context;

    public CaseCompleteReportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CaseCompleteReportViewModel?> GetCaseCompleteReportAsync(long caseId)
    {
        var pvCase = await _context.PvCases
            .FirstOrDefaultAsync(x => x.Id == caseId && !x.IsDeleted);

        if (pvCase == null)
        {
            return null;
        }

        var patient = await _context.CasePatientDetails
            .FirstOrDefaultAsync(x => x.PvCaseId == caseId && !x.IsDeleted);

        var reporter = await _context.CaseReporterDetails
            .FirstOrDefaultAsync(x => x.PvCaseId == caseId && !x.IsDeleted);

        var adverseEvent = await _context.CaseAdverseEventDetails
            .FirstOrDefaultAsync(x => x.PvCaseId == caseId && !x.IsDeleted);

        var suspectProduct = await _context.CaseSuspectProductDetails
            .FirstOrDefaultAsync(x => x.PvCaseId == caseId && !x.IsDeleted);

        var concomitantMedications = await _context.CaseConcomitantMedications
            .Where(x => x.PvCaseId == caseId && !x.IsDeleted)
            .OrderBy(x => x.Id)
            .ToListAsync();

        var labDetails = await _context.CaseLabDetails
            .Where(x => x.PvCaseId == caseId && !x.IsDeleted)
            .OrderBy(x => x.TestDate)
            .ThenBy(x => x.Id)
            .ToListAsync();

        var model = new CaseCompleteReportViewModel
        {
            PvCaseId = pvCase.Id,
            CaseNo = pvCase.CaseNo,
            CaseSource = pvCase.CaseSource,
            ReceiptDate = pvCase.ReceiptDate,
            Status = pvCase.Status,
            IsValidCase = pvCase.IsValidCase,
            IsSerious = pvCase.IsSerious,
            DueDate = pvCase.DueDate,
            InitialReporterName = pvCase.InitialReporterName,
            InitialPatientIdentifier = pvCase.InitialPatientIdentifier,
            InitialProductName = pvCase.InitialProductName,
            InitialEventTerm = pvCase.InitialEventTerm,
            InitialNarrative = pvCase.Narrative,
            Narrative = pvCase.CaseNarrativeHtml,

            Patient = new PatientReportSection
            {
                PatientIdentifier = patient?.PatientIdentifier,
                PatientInitials = patient?.PatientInitials,
                DateOfBirth = patient?.DateOfBirth,
                Age = patient?.Age,
                AgeUnit = patient?.AgeUnit,
                Gender = patient?.Gender,
                WeightKg = patient?.WeightKg,
                HeightCm = patient?.HeightCm,
                IsPregnant = patient?.IsPregnant ?? false,
                PregnancyRemarks = patient?.PregnancyRemarks,
                RelevantMedicalHistory = patient?.RelevantMedicalHistory,
                AllergyHistory = patient?.AllergyHistory,
                OtherPatientInformation = patient?.OtherPatientInformation
            },

            Reporter = new ReporterReportSection
            {
                ReporterName = reporter?.ReporterName,
                ReporterType = reporter?.ReporterType,
                Qualification = reporter?.Qualification,
                OrganizationName = reporter?.OrganizationName,
                Department = reporter?.Department,
                Email = reporter?.Email,
                Phone = reporter?.Phone,
                Address = reporter?.Address,
                City = reporter?.City,
                State = reporter?.State,
                Country = reporter?.Country,
                DateOfReport = reporter?.DateOfReport,
                ConsentForFollowUp = reporter?.ConsentForFollowUp ?? false,
                ReporterRemarks = reporter?.ReporterRemarks
            },

            AdverseEvent = new AdverseEventReportSection
            {
                EventTerm = adverseEvent?.EventTerm,
                EventStartDate = adverseEvent?.EventStartDate,
                EventEndDate = null,
                Outcome = adverseEvent?.Outcome,
                IsSerious = adverseEvent?.IsSerious ?? pvCase.IsSerious,
                SeriousnessCriteria = adverseEvent?.SeriousnessCriteria,
                EventDescription = adverseEvent?.EventDescription,
                TreatmentGiven = null,
                EventRemarks = null
            },

            SuspectProduct = new SuspectProductReportSection
            {
                ProductName = suspectProduct?.ProductName,
                GenericName = suspectProduct?.GenericName,
                BatchNo = suspectProduct?.BatchNo,
                ExpiryDate = suspectProduct?.ExpiryDate,
                Dose = suspectProduct?.Dose,
                DoseUnit = suspectProduct?.DoseUnit,
                Route = suspectProduct?.Route,
                Frequency = suspectProduct?.Frequency,
                TherapyStartDate = suspectProduct?.TherapyStartDate,
                TherapyStopDate = suspectProduct?.TherapyStopDate,
                Indication = suspectProduct?.Indication,
                ActionTaken = suspectProduct?.ActionTaken,
                Dechallenge = suspectProduct?.Dechallenge,
                Rechallenge = suspectProduct?.Rechallenge,
                CausalityAssessment = suspectProduct?.Causality,
                ProductRemarks = suspectProduct?.ProductRemarks
            },

            ConcomitantMedications = concomitantMedications.Select(x => new ConcomitantMedicationReportSection
            {
                MedicationName = x.MedicationName,
                Dose = x.Dose,
                DoseUnit = x.DoseUnit,
                Route = x.Route,
                Frequency = x.Frequency,
                TherapyStartDate = x.TherapyStartDate,
                TherapyStopDate = x.TherapyStopDate,
                Indication = x.Indication,
                IsMedicationForEventTreatment = x.IsMedicationForEventTreatment,
                Remarks = x.Remarks
            }).ToList(),

            LabDetails = labDetails.Select(x => new LabDetailReportSection
            {
                TestName = x.TestName,
                TestDate = x.TestDate,
                ResultValue = null,
                Unit = x.Unit,
                NormalRange = x.NormalRange,
                ClinicalSignificance = x.ClinicalSignificance,
                Remarks = x.Remarks
            }).ToList()
        };

        return model;
    }
}