using InnoPV.Domain.Entities;
using InnoPV.Domain.Enums;
using InnoPV.Domain.Identity;
using InnoPV.Infrastructure.Data;
using InnoPV.Web.Models.PvCaseEntry;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InnoPV.Web.Services.Security;

namespace InnoPV.Web.Controllers;

[Authorize(Roles = AppRoles.Admin + "," + AppRoles.PvAssociate + "," + AppRoles.PvManager + "," + AppRoles.MedicalReviewer)]
public class PvCaseEntryController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _environment;
    private readonly ICaseSecurityService _caseSecurityService;

    public PvCaseEntryController(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    IWebHostEnvironment environment,
    ICaseSecurityService caseSecurityService)
    {
        _context = context;
        _userManager = userManager;
        _environment = environment;
        _caseSecurityService = caseSecurityService;
    }

    [HttpGet]
    public async Task<IActionResult> Patient(long caseId)
    {
        if (!await EnsureCaseViewableAsync(caseId))
        {
            return RedirectToAction("Index", "CaseInbox");
        }
        var pvCase = await GetCaseAsync(caseId);

        if (pvCase == null)
        {
            TempData["ErrorMessage"] = "Case not found.";
            return RedirectToAction("Index", "PvCase");
        }

        var entity = await _context.CasePatientDetails
            .FirstOrDefaultAsync(x => x.PvCaseId == caseId && !x.IsDeleted);

        var model = new CasePatientDetailViewModel
        {
            Id = entity?.Id ?? 0,
            PvCaseId = caseId,
            CaseNo = pvCase.CaseNo,
            IsReadOnly = _caseSecurityService.IsCaseReadOnly(pvCase),
            PatientInitials = entity?.PatientInitials,
            PatientIdentifier = entity?.PatientIdentifier,
            DateOfBirth = entity?.DateOfBirth,
            Age = entity?.Age,
            AgeUnit = entity?.AgeUnit,
            Gender = entity?.Gender,
            WeightKg = entity?.WeightKg,
            HeightCm = entity?.HeightCm,
            IsPregnant = entity?.IsPregnant ?? false,
            PregnancyRemarks = entity?.PregnancyRemarks,
            RelevantMedicalHistory = entity?.RelevantMedicalHistory,
            AllergyHistory = entity?.AllergyHistory,
            OtherPatientInformation = entity?.OtherPatientInformation
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Patient(CasePatientDetailViewModel model)
    {
        if (!await EnsureCaseEditableAsync(model.PvCaseId))
        {
            return RedirectToAction("Index", "CaseInbox");
        }
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var pvCase = await GetCaseAsync(model.PvCaseId);

        if (pvCase == null)
        {
            TempData["ErrorMessage"] = "Case not found.";
            return RedirectToAction("Index", "PvCase");
        }

        var currentUserId = _userManager.GetUserId(User);

        var entity = await _context.CasePatientDetails
            .FirstOrDefaultAsync(x => x.PvCaseId == model.PvCaseId && !x.IsDeleted);

        if (entity == null)
        {
            entity = new CasePatientDetail
            {
                PvCaseId = model.PvCaseId,
                CreatedBy = currentUserId,
                CreatedOnUtc = DateTime.UtcNow
            };

            _context.CasePatientDetails.Add(entity);
        }
        else
        {
            entity.ModifiedBy = currentUserId;
            entity.ModifiedOnUtc = DateTime.UtcNow;
        }

        entity.PatientInitials = model.PatientInitials?.Trim();
        entity.PatientIdentifier = model.PatientIdentifier?.Trim();
        entity.DateOfBirth = model.DateOfBirth;
        entity.Age = model.Age;
        entity.AgeUnit = model.AgeUnit?.Trim();
        entity.Gender = model.Gender?.Trim();
        entity.WeightKg = model.WeightKg;
        entity.HeightCm = model.HeightCm;
        entity.IsPregnant = model.IsPregnant;
        entity.PregnancyRemarks = model.PregnancyRemarks?.Trim();
        entity.RelevantMedicalHistory = model.RelevantMedicalHistory?.Trim();
        entity.AllergyHistory = model.AllergyHistory?.Trim();
        entity.OtherPatientInformation = model.OtherPatientInformation?.Trim();

        if (!string.IsNullOrWhiteSpace(model.PatientIdentifier))
        {
            pvCase.InitialPatientIdentifier = model.PatientIdentifier.Trim();
            pvCase.IsPatientIdentifiable = true;
        }

        UpdateCaseValidity(pvCase);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Patient details saved successfully.";
        return RedirectToAction(nameof(Patient), new { caseId = model.PvCaseId });
    }

    [HttpGet]
    public async Task<IActionResult> Reporter(long caseId)
    {
        if (!await EnsureCaseViewableAsync(caseId))
        {
            return RedirectToAction("Index", "CaseInbox");
        }
        var pvCase = await GetCaseAsync(caseId);

        if (pvCase == null)
        {
            TempData["ErrorMessage"] = "Case not found.";
            return RedirectToAction("Index", "PvCase");
        }

        var entity = await _context.CaseReporterDetails
            .FirstOrDefaultAsync(x => x.PvCaseId == caseId && !x.IsDeleted);

        var model = new CaseReporterDetailViewModel
        {
            Id = entity?.Id ?? 0,
            PvCaseId = caseId,
            CaseNo = pvCase.CaseNo,
            ReporterName = entity?.ReporterName ?? pvCase.InitialReporterName ?? string.Empty,
            ReporterType = entity?.ReporterType,
            Qualification = entity?.Qualification,
            OrganizationName = entity?.OrganizationName,
            Department = entity?.Department,
            Email = entity?.Email,
            Phone = entity?.Phone,
            Address = entity?.Address,
            City = entity?.City,
            State = entity?.State,
            Country = entity?.Country,
            DateOfReport = entity?.DateOfReport,
            ConsentForFollowUp = entity?.ConsentForFollowUp ?? false,
            ReporterRemarks = entity?.ReporterRemarks
        };

        await LoadReporterDropdownsAsync(model);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reporter(CaseReporterDetailViewModel model)
    {
        if (!await EnsureCaseEditableAsync(model.PvCaseId))
        {
            return RedirectToAction("Index", "CaseInbox");
        }
        await LoadReporterDropdownsAsync(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var pvCase = await GetCaseAsync(model.PvCaseId);

        if (pvCase == null)
        {
            TempData["ErrorMessage"] = "Case not found.";
            return RedirectToAction("Index", "PvCase");
        }

        var currentUserId = _userManager.GetUserId(User);

        var entity = await _context.CaseReporterDetails
            .FirstOrDefaultAsync(x => x.PvCaseId == model.PvCaseId && !x.IsDeleted);

        if (entity == null)
        {
            entity = new CaseReporterDetail
            {
                PvCaseId = model.PvCaseId,
                CreatedBy = currentUserId,
                CreatedOnUtc = DateTime.UtcNow
            };

            _context.CaseReporterDetails.Add(entity);
        }
        else
        {
            entity.ModifiedBy = currentUserId;
            entity.ModifiedOnUtc = DateTime.UtcNow;
        }

        entity.ReporterName = model.ReporterName.Trim();
        entity.ReporterType = model.ReporterType?.Trim();
        entity.Qualification = model.Qualification?.Trim();
        entity.OrganizationName = model.OrganizationName?.Trim();
        entity.Department = model.Department?.Trim();
        entity.Email = model.Email?.Trim();
        entity.Phone = model.Phone?.Trim();
        entity.Address = model.Address?.Trim();
        entity.City = model.City?.Trim();
        entity.State = model.State?.Trim();
        entity.Country = model.Country?.Trim();
        entity.DateOfReport = model.DateOfReport;
        entity.ConsentForFollowUp = model.ConsentForFollowUp;
        entity.ReporterRemarks = model.ReporterRemarks?.Trim();

        pvCase.InitialReporterName = model.ReporterName.Trim();
        pvCase.IsReporterIdentifiable = true;

        UpdateCaseValidity(pvCase);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Reporter details saved successfully.";
        return RedirectToAction(nameof(Reporter), new { caseId = model.PvCaseId });
    }

    [HttpGet]
    public async Task<IActionResult> AdverseEvent(long caseId)
    {
        if (!await EnsureCaseViewableAsync(caseId))
        {
            return RedirectToAction("Index", "CaseInbox");
        }
        var pvCase = await GetCaseAsync(caseId);

        if (pvCase == null)
        {
            TempData["ErrorMessage"] = "Case not found.";
            return RedirectToAction("Index", "PvCase");
        }

        var entity = await _context.CaseAdverseEventDetails
            .FirstOrDefaultAsync(x => x.PvCaseId == caseId && !x.IsDeleted);

        var model = new CaseAdverseEventDetailViewModel
        {
            Id = entity?.Id ?? 0,
            PvCaseId = caseId,
            CaseNo = pvCase.CaseNo,
            IsReadOnly = _caseSecurityService.IsCaseReadOnly(pvCase),
            EventTerm = entity?.EventTerm ?? pvCase.InitialEventTerm ?? string.Empty,
            EventDescription = entity?.EventDescription,
            EventStartDate = entity?.EventStartDate,
            EventStopDate = entity?.EventStopDate,
            IsSerious = entity?.IsSerious ?? pvCase.IsSerious,
            SeriousnessCriteria = entity?.SeriousnessCriteria,
            Severity = entity?.Severity,
            Outcome = entity?.Outcome,
            DeathDate = entity?.DeathDate,
            CauseOfDeath = entity?.CauseOfDeath,
            WasHospitalized = entity?.WasHospitalized ?? false,
            HospitalizationDate = entity?.HospitalizationDate,
            DischargeDate = entity?.DischargeDate,
            TreatmentGivenForEvent = entity?.TreatmentGivenForEvent,
            EventRemarks = entity?.EventRemarks
        };

        await LoadAdverseEventDropdownsAsync(model);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdverseEvent(CaseAdverseEventDetailViewModel model)
    {
        if (!await EnsureCaseEditableAsync(model.PvCaseId))
        {
            return RedirectToAction("Index", "CaseInbox");
        }
        await LoadAdverseEventDropdownsAsync(model);

        if (model.EventStopDate.HasValue &&
            model.EventStartDate.HasValue &&
            model.EventStopDate.Value.Date < model.EventStartDate.Value.Date)
        {
            ModelState.AddModelError(nameof(model.EventStopDate), "Event stop date cannot be before event start date.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var pvCase = await GetCaseAsync(model.PvCaseId);

        if (pvCase == null)
        {
            TempData["ErrorMessage"] = "Case not found.";
            return RedirectToAction("Index", "PvCase");
        }

        var currentUserId = _userManager.GetUserId(User);

        var entity = await _context.CaseAdverseEventDetails
            .FirstOrDefaultAsync(x => x.PvCaseId == model.PvCaseId && !x.IsDeleted);

        if (entity == null)
        {
            entity = new CaseAdverseEventDetail
            {
                PvCaseId = model.PvCaseId,
                CreatedBy = currentUserId,
                CreatedOnUtc = DateTime.UtcNow
            };

            _context.CaseAdverseEventDetails.Add(entity);
        }
        else
        {
            entity.ModifiedBy = currentUserId;
            entity.ModifiedOnUtc = DateTime.UtcNow;
        }

        entity.EventTerm = model.EventTerm.Trim();
        entity.EventDescription = model.EventDescription?.Trim();
        entity.EventStartDate = model.EventStartDate;
        entity.EventStopDate = model.EventStopDate;
        entity.IsSerious = model.IsSerious;
        entity.SeriousnessCriteria = model.SeriousnessCriteria?.Trim();
        entity.Severity = model.Severity?.Trim();
        entity.Outcome = model.Outcome?.Trim();
        entity.DeathDate = model.DeathDate;
        entity.CauseOfDeath = model.CauseOfDeath?.Trim();
        entity.WasHospitalized = model.WasHospitalized;
        entity.HospitalizationDate = model.HospitalizationDate;
        entity.DischargeDate = model.DischargeDate;
        entity.TreatmentGivenForEvent = model.TreatmentGivenForEvent?.Trim();
        entity.EventRemarks = model.EventRemarks?.Trim();

        pvCase.InitialEventTerm = model.EventTerm.Trim();
        pvCase.IsAdverseEventAvailable = true;
        pvCase.IsSerious = model.IsSerious;
        pvCase.DueDate = CalculateDueDate(pvCase.ReceiptDate, model.IsSerious);

        UpdateCaseValidity(pvCase);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Adverse event details saved successfully.";
        return RedirectToAction(nameof(AdverseEvent), new { caseId = model.PvCaseId });
    }

    [HttpGet]
    public async Task<IActionResult> SuspectProduct(long caseId)
    {
        if (!await EnsureCaseViewableAsync(caseId))
        {
            return RedirectToAction("Index", "CaseInbox");
        }
        var pvCase = await GetCaseAsync(caseId);

        if (pvCase == null)
        {
            TempData["ErrorMessage"] = "Case not found.";
            return RedirectToAction("Index", "PvCase");
        }

        var entity = await _context.CaseSuspectProductDetails
            .FirstOrDefaultAsync(x => x.PvCaseId == caseId && !x.IsDeleted);

        var model = new CaseSuspectProductDetailViewModel
        {
            Id = entity?.Id ?? 0,
            PvCaseId = caseId,
            CaseNo = pvCase.CaseNo,
            IsReadOnly = _caseSecurityService.IsCaseReadOnly(pvCase),
            ProductName = entity?.ProductName ?? pvCase.InitialProductName ?? string.Empty,
            GenericName = entity?.GenericName,
            BatchNo = entity?.BatchNo,
            ExpiryDate = entity?.ExpiryDate,
            Dose = entity?.Dose,
            DoseUnit = entity?.DoseUnit,
            Route = entity?.Route,
            Frequency = entity?.Frequency,
            TherapyStartDate = entity?.TherapyStartDate,
            TherapyStopDate = entity?.TherapyStopDate,
            Indication = entity?.Indication,
            ActionTaken = entity?.ActionTaken,
            Dechallenge = entity?.Dechallenge,
            Rechallenge = entity?.Rechallenge,
            Causality = entity?.Causality,
            ProductRemarks = entity?.ProductRemarks
        };

        await LoadSuspectProductDropdownsAsync(model);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SuspectProduct(CaseSuspectProductDetailViewModel model)
    {
        if (!await EnsureCaseEditableAsync(model.PvCaseId))
        {
            return RedirectToAction("Index", "CaseInbox");
        }
        await LoadSuspectProductDropdownsAsync(model);

        if (model.TherapyStopDate.HasValue &&
            model.TherapyStartDate.HasValue &&
            model.TherapyStopDate.Value.Date < model.TherapyStartDate.Value.Date)
        {
            ModelState.AddModelError(nameof(model.TherapyStopDate), "Therapy stop date cannot be before therapy start date.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var pvCase = await GetCaseAsync(model.PvCaseId);

        if (pvCase == null)
        {
            TempData["ErrorMessage"] = "Case not found.";
            return RedirectToAction("Index", "PvCase");
        }

        var currentUserId = _userManager.GetUserId(User);

        var entity = await _context.CaseSuspectProductDetails
            .FirstOrDefaultAsync(x => x.PvCaseId == model.PvCaseId && !x.IsDeleted);

        if (entity == null)
        {
            entity = new CaseSuspectProductDetail
            {
                PvCaseId = model.PvCaseId,
                CreatedBy = currentUserId,
                CreatedOnUtc = DateTime.UtcNow
            };

            _context.CaseSuspectProductDetails.Add(entity);
        }
        else
        {
            entity.ModifiedBy = currentUserId;
            entity.ModifiedOnUtc = DateTime.UtcNow;
        }

        entity.ProductName = model.ProductName.Trim();
        entity.GenericName = model.GenericName?.Trim();
        entity.BatchNo = model.BatchNo?.Trim();
        entity.ExpiryDate = model.ExpiryDate;
        entity.Dose = model.Dose?.Trim();
        entity.DoseUnit = model.DoseUnit?.Trim();
        entity.Route = model.Route?.Trim();
        entity.Frequency = model.Frequency?.Trim();
        entity.TherapyStartDate = model.TherapyStartDate;
        entity.TherapyStopDate = model.TherapyStopDate;
        entity.Indication = model.Indication?.Trim();
        entity.ActionTaken = model.ActionTaken?.Trim();
        entity.Dechallenge = model.Dechallenge?.Trim();
        entity.Rechallenge = model.Rechallenge?.Trim();
        entity.Causality = model.Causality?.Trim();
        entity.ProductRemarks = model.ProductRemarks?.Trim();

        pvCase.InitialProductName = model.ProductName.Trim();
        pvCase.IsSuspectProductAvailable = true;

        UpdateCaseValidity(pvCase);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Suspect product details saved successfully.";
        return RedirectToAction(nameof(SuspectProduct), new { caseId = model.PvCaseId });
    }

    [HttpGet]
    public async Task<IActionResult> ConcomitantMedications(long caseId)
    {
        if (!await EnsureCaseViewableAsync(caseId))
        {
            return RedirectToAction("Index", "CaseInbox");
        }
        var pvCase = await GetCaseAsync(caseId);

        if (pvCase == null)
        {
            TempData["ErrorMessage"] = "Case not found.";
            return RedirectToAction("Index", "PvCase");
        }

        ViewBag.PvCaseId = caseId;
        ViewBag.CaseNo = pvCase.CaseNo;
        ViewBag.IsReadOnly = _caseSecurityService.IsCaseReadOnly(pvCase);

        var list = await _context.CaseConcomitantMedications
            .Where(x => x.PvCaseId == caseId && !x.IsDeleted)
            .OrderBy(x => x.MedicationName)
            .Select(x => new CaseConcomitantMedicationViewModel
            {
                Id = x.Id,
                PvCaseId = x.PvCaseId,
                CaseNo = pvCase.CaseNo,
                IsReadOnly = _caseSecurityService.IsCaseReadOnly(pvCase),
                MedicationName = x.MedicationName,
                Dose = x.Dose,
                DoseUnit = x.DoseUnit,
                Route = x.Route,
                Frequency = x.Frequency,
                TherapyStartDate = x.TherapyStartDate,
                TherapyStopDate = x.TherapyStopDate,
                Indication = x.Indication,
                IsMedicationForEventTreatment = x.IsMedicationForEventTreatment,
                Remarks = x.Remarks,
                IsActive = x.IsActive
            })
            .ToListAsync();

        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> CreateConcomitantMedication(long caseId)
    {
        if (!await EnsureCaseViewableAsync(caseId))
        {
            return RedirectToAction("Index", "CaseInbox");
        }
        var pvCase = await GetCaseAsync(caseId);

        if (pvCase == null)
        {
            TempData["ErrorMessage"] = "Case not found.";
            return RedirectToAction("Index", "PvCase");
        }

        if (_caseSecurityService.IsCaseReadOnly(pvCase))
        {
            TempData["ErrorMessage"] = "This case is read-only. New medication cannot be added.";
            return RedirectToAction(nameof(ConcomitantMedications), new { caseId });
        }

        var model = new CaseConcomitantMedicationViewModel
        {
            PvCaseId = caseId,
            CaseNo = pvCase.CaseNo,
            IsActive = true,
            IsReadOnly = _caseSecurityService.IsCaseReadOnly(pvCase)
        };

        await LoadMedicationDropdownsAsync(model);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateConcomitantMedication(CaseConcomitantMedicationViewModel model)
    {
        if (!await EnsureCaseEditableAsync(model.PvCaseId))
        {
            return RedirectToAction("Index", "CaseInbox");
        }
        await LoadMedicationDropdownsAsync(model);

        if (model.TherapyStopDate.HasValue &&
            model.TherapyStartDate.HasValue &&
            model.TherapyStopDate.Value.Date < model.TherapyStartDate.Value.Date)
        {
            ModelState.AddModelError(nameof(model.TherapyStopDate), "Therapy stop date cannot be before therapy start date.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var currentUserId = _userManager.GetUserId(User);

        var entity = new CaseConcomitantMedication
        {
            PvCaseId = model.PvCaseId,
            MedicationName = model.MedicationName.Trim(),
            Dose = model.Dose?.Trim(),
            DoseUnit = model.DoseUnit?.Trim(),
            Route = model.Route?.Trim(),
            Frequency = model.Frequency?.Trim(),
            TherapyStartDate = model.TherapyStartDate,
            TherapyStopDate = model.TherapyStopDate,
            Indication = model.Indication?.Trim(),
            IsMedicationForEventTreatment = model.IsMedicationForEventTreatment,
            Remarks = model.Remarks?.Trim(),
            IsActive = model.IsActive,
            CreatedBy = currentUserId,
            CreatedOnUtc = DateTime.UtcNow
        };

        _context.CaseConcomitantMedications.Add(entity);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Concomitant medication saved successfully.";
        return RedirectToAction(nameof(ConcomitantMedications), new { caseId = model.PvCaseId });
    }

    [HttpGet]
    public async Task<IActionResult> EditConcomitantMedication(long id)
    {
        var entity = await _context.CaseConcomitantMedications
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (entity == null)
        {
            TempData["ErrorMessage"] = "Concomitant medication not found.";
            return RedirectToAction("Index", "CaseInbox");
        }

        if (!await EnsureCaseViewableAsync(entity.PvCaseId))
        {
            return RedirectToAction("Index", "CaseInbox");
        }

        var pvCase = await GetCaseAsync(entity.PvCaseId);

        if (pvCase == null)
        {
            TempData["ErrorMessage"] = "Case not found.";
            return RedirectToAction("Index", "CaseInbox");
        }

        var model = new CaseConcomitantMedicationViewModel
        {
            Id = entity.Id,
            PvCaseId = entity.PvCaseId,
            CaseNo = pvCase.CaseNo,
            IsReadOnly = _caseSecurityService.IsCaseReadOnly(pvCase),
            MedicationName = entity.MedicationName,
            Dose = entity.Dose,
            DoseUnit = entity.DoseUnit,
            Route = entity.Route,
            Frequency = entity.Frequency,
            TherapyStartDate = entity.TherapyStartDate,
            TherapyStopDate = entity.TherapyStopDate,
            Indication = entity.Indication,
            IsMedicationForEventTreatment = entity.IsMedicationForEventTreatment,
            Remarks = entity.Remarks,
            IsActive = entity.IsActive
        };

        await LoadMedicationDropdownsAsync(model);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditConcomitantMedication(CaseConcomitantMedicationViewModel model)
    {
        if (!await EnsureCaseEditableAsync(model.PvCaseId))
        {
            return RedirectToAction("Index", "CaseInbox");
        }

        await LoadMedicationDropdownsAsync(model);

        if (model.TherapyStopDate.HasValue &&
            model.TherapyStartDate.HasValue &&
            model.TherapyStopDate.Value.Date < model.TherapyStartDate.Value.Date)
        {
            ModelState.AddModelError(nameof(model.TherapyStopDate), "Therapy stop date cannot be before therapy start date.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var entity = await _context.CaseConcomitantMedications
            .FirstOrDefaultAsync(x => x.Id == model.Id && x.PvCaseId == model.PvCaseId && !x.IsDeleted);

        if (entity == null)
        {
            TempData["ErrorMessage"] = "Concomitant medication not found.";
            return RedirectToAction(nameof(ConcomitantMedications), new { caseId = model.PvCaseId });
        }

        var currentUserId = _userManager.GetUserId(User);

        entity.MedicationName = model.MedicationName.Trim();
        entity.Dose = model.Dose?.Trim();
        entity.DoseUnit = model.DoseUnit?.Trim();
        entity.Route = model.Route?.Trim();
        entity.Frequency = model.Frequency?.Trim();
        entity.TherapyStartDate = model.TherapyStartDate;
        entity.TherapyStopDate = model.TherapyStopDate;
        entity.Indication = model.Indication?.Trim();
        entity.IsMedicationForEventTreatment = model.IsMedicationForEventTreatment;
        entity.Remarks = model.Remarks?.Trim();
        entity.IsActive = model.IsActive;
        entity.ModifiedBy = currentUserId;
        entity.ModifiedOnUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Concomitant medication updated successfully.";
        return RedirectToAction(nameof(ConcomitantMedications), new { caseId = model.PvCaseId });
    }

    [HttpGet]
    public async Task<IActionResult> LabDetails(long caseId)
    {
        if (!await EnsureCaseViewableAsync(caseId))
        {
            return RedirectToAction("Index", "CaseInbox");
        }
        var pvCase = await GetCaseAsync(caseId);
       

        if (pvCase == null)
        {
            TempData["ErrorMessage"] = "Case not found.";
            return RedirectToAction("Index", "PvCase");
        }

        ViewBag.PvCaseId = caseId;
        ViewBag.CaseNo = pvCase.CaseNo;
        ViewBag.IsReadOnly = _caseSecurityService.IsCaseReadOnly(pvCase);

        var list = await _context.CaseLabDetails
            .Where(x => x.PvCaseId == caseId && !x.IsDeleted)
            .OrderBy(x => x.TestDate)
            .Select(x => new CaseLabDetailViewModel
            {
                Id = x.Id,
                PvCaseId = x.PvCaseId,
                CaseNo = pvCase.CaseNo,
                IsReadOnly = _caseSecurityService.IsCaseReadOnly(pvCase),
                TestName = x.TestName,
                TestDate = x.TestDate,
                TestResult = x.TestResult,
                Unit = x.Unit,
                NormalRange = x.NormalRange,
                ClinicalSignificance = x.ClinicalSignificance,
                Remarks = x.Remarks,
                IsActive = x.IsActive
            })
            .ToListAsync();

        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> CreateLabDetail(long caseId)
    {
        if (!await EnsureCaseViewableAsync(caseId))
        {
            return RedirectToAction("Index", "CaseInbox");
        }
        var pvCase = await GetCaseAsync(caseId);

        if (pvCase == null)
        {
            TempData["ErrorMessage"] = "Case not found.";
            return RedirectToAction("Index", "PvCase");
        }

        if (_caseSecurityService.IsCaseReadOnly(pvCase))
        {
            TempData["ErrorMessage"] = "This case is read-only. New lab detail cannot be added.";
            return RedirectToAction(nameof(LabDetails), new { caseId });
        }

        return View(new CaseLabDetailViewModel
        {
            PvCaseId = caseId,
            CaseNo = pvCase.CaseNo,
            IsActive = true,
            IsReadOnly = _caseSecurityService.IsCaseReadOnly(pvCase)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateLabDetail(CaseLabDetailViewModel model)
    {
        if (!await EnsureCaseEditableAsync(model.PvCaseId))
        {
            return RedirectToAction("Index", "CaseInbox");
        }
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var currentUserId = _userManager.GetUserId(User);

        var entity = new CaseLabDetail
        {
            PvCaseId = model.PvCaseId,
            TestName = model.TestName.Trim(),
            TestDate = model.TestDate,
            TestResult = model.TestResult?.Trim(),
            Unit = model.Unit?.Trim(),
            NormalRange = model.NormalRange?.Trim(),
            ClinicalSignificance = model.ClinicalSignificance?.Trim(),
            Remarks = model.Remarks?.Trim(),
            IsActive = model.IsActive,
            CreatedBy = currentUserId,
            CreatedOnUtc = DateTime.UtcNow
        };

        _context.CaseLabDetails.Add(entity);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Lab detail saved successfully.";
        return RedirectToAction(nameof(LabDetails), new { caseId = model.PvCaseId });
    }

    [HttpGet]
    public async Task<IActionResult> EditLabDetail(long id)
    {
        var entity = await _context.CaseLabDetails
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (entity == null)
        {
            TempData["ErrorMessage"] = "Lab detail not found.";
            return RedirectToAction("Index", "CaseInbox");
        }

        if (!await EnsureCaseViewableAsync(entity.PvCaseId))
        {
            return RedirectToAction("Index", "CaseInbox");
        }

        var pvCase = await GetCaseAsync(entity.PvCaseId);

        if (pvCase == null)
        {
            TempData["ErrorMessage"] = "Case not found.";
            return RedirectToAction("Index", "CaseInbox");
        }

        var model = new CaseLabDetailViewModel
        {
            Id = entity.Id,
            PvCaseId = entity.PvCaseId,
            CaseNo = pvCase.CaseNo,
            IsReadOnly = _caseSecurityService.IsCaseReadOnly(pvCase),
            TestName = entity.TestName,
            TestDate = entity.TestDate,
            TestResult = entity.TestResult,
            Unit = entity.Unit,
            NormalRange = entity.NormalRange,
            ClinicalSignificance = entity.ClinicalSignificance,
            Remarks = entity.Remarks,
            IsActive = entity.IsActive
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditLabDetail(CaseLabDetailViewModel model)
    {
        if (!await EnsureCaseEditableAsync(model.PvCaseId))
        {
            return RedirectToAction("Index", "CaseInbox");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var entity = await _context.CaseLabDetails
            .FirstOrDefaultAsync(x => x.Id == model.Id && x.PvCaseId == model.PvCaseId && !x.IsDeleted);

        if (entity == null)
        {
            TempData["ErrorMessage"] = "Lab detail not found.";
            return RedirectToAction(nameof(LabDetails), new { caseId = model.PvCaseId });
        }

        var currentUserId = _userManager.GetUserId(User);

        entity.TestName = model.TestName.Trim();
        entity.TestDate = model.TestDate;
        entity.TestResult = model.TestResult?.Trim();
        entity.Unit = model.Unit?.Trim();
        entity.NormalRange = model.NormalRange?.Trim();
        entity.ClinicalSignificance = model.ClinicalSignificance?.Trim();
        entity.Remarks = model.Remarks?.Trim();
        entity.IsActive = model.IsActive;
        entity.ModifiedBy = currentUserId;
        entity.ModifiedOnUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Lab detail updated successfully.";
        return RedirectToAction(nameof(LabDetails), new { caseId = model.PvCaseId });
    }

    [HttpGet]
    public async Task<IActionResult> Attachments(long caseId)
    {
        if (!await EnsureCaseViewableAsync(caseId))
        {
            return RedirectToAction("Index", "CaseInbox");
        }
        var pvCase = await GetCaseAsync(caseId);

        if (pvCase == null)
        {
            TempData["ErrorMessage"] = "Case not found.";
            return RedirectToAction("Index", "PvCase");
        }

        ViewBag.PvCaseId = caseId;
        ViewBag.CaseNo = pvCase.CaseNo;
        ViewBag.IsReadOnly = _caseSecurityService.IsCaseReadOnly(pvCase);

        var list = await _context.CaseAttachments
            .Where(x => x.PvCaseId == caseId && !x.IsDeleted)
            .OrderByDescending(x => x.UploadedOnUtc)
            .Select(x => new CaseAttachmentViewModel
            {
                Id = x.Id,
                PvCaseId = x.PvCaseId,
                CaseNo = pvCase.CaseNo,
                AttachmentType = x.AttachmentType,
                OriginalFileName = x.OriginalFileName,
                StoredFileName = x.StoredFileName,
                FilePath = x.FilePath,
                ContentType = x.ContentType,
                FileSizeBytes = x.FileSizeBytes,
                Description = x.Description,
                UploadedOnUtc = x.UploadedOnUtc
            })
            .ToListAsync();

        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> UploadAttachment(long caseId)
    {
        if (!await EnsureCaseViewableAsync(caseId))
        {
            return RedirectToAction("Index", "CaseInbox");
        }
        var pvCase = await GetCaseAsync(caseId);


        if (pvCase == null)
        {
            TempData["ErrorMessage"] = "Case not found.";
            return RedirectToAction("Index", "PvCase");
        }

        if (_caseSecurityService.IsCaseReadOnly(pvCase))
        {
            TempData["ErrorMessage"] = "This case is read-only. Attachment upload is not allowed.";
            return RedirectToAction(nameof(Attachments), new { caseId });
        }

        return View(new CaseAttachmentUploadViewModel
        {
            PvCaseId = caseId,
            CaseNo = pvCase.CaseNo,
            IsReadOnly = _caseSecurityService.IsCaseReadOnly(pvCase)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadAttachment(CaseAttachmentUploadViewModel model)
    {
        if (!await EnsureCaseEditableAsync(model.PvCaseId))
        {
            return RedirectToAction("Index", "CaseInbox");
        }
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.File == null || model.File.Length == 0)
        {
            ModelState.AddModelError(nameof(model.File), "Please select a valid file.");
            return View(model);
        }

        var originalFileName = Path.GetFileName(model.File.FileName);

        if (string.IsNullOrWhiteSpace(originalFileName))
        {
            ModelState.AddModelError(nameof(model.File), "Invalid file name.");
            return View(model);
        }

        var blockedFileNameChars = Path.GetInvalidFileNameChars();

        if (originalFileName.Any(ch => blockedFileNameChars.Contains(ch)))
        {
            ModelState.AddModelError(nameof(model.File), "File name contains invalid characters.");
            return View(model);
        }

        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();

        var allowedExtensions = new[]
        {
    ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx", ".xls", ".xlsx"
};

        if (string.IsNullOrWhiteSpace(extension))
        {
            ModelState.AddModelError(nameof(model.File), "File extension is required.");
            return View(model);
        }

        if (!allowedExtensions.Contains(extension))
        {
            ModelState.AddModelError(nameof(model.File), "Only PDF, image, Word and Excel files are allowed.");
            return View(model);
        }

        const long maxFileSize = 10 * 1024 * 1024;

        if (model.File.Length > maxFileSize)
        {
            ModelState.AddModelError(nameof(model.File), "Maximum file size allowed is 10 MB.");
            return View(model);
        }

        var pvCase = await GetCaseAsync(model.PvCaseId);

        if (pvCase == null)
        {
            TempData["ErrorMessage"] = "Case not found.";
            return RedirectToAction("Index", "PvCase");
        }

        var uploadFolder = GetPrivateUploadFolder("pvcases", model.PvCaseId.ToString());

        if (!Directory.Exists(uploadFolder))
        {
            Directory.CreateDirectory(uploadFolder);
        }

        var storedFileName = await GenerateAttachmentFileNameAsync(
            model.PvCaseId,
            pvCase.CaseNo,
            extension,
            uploadFolder);
        var physicalPath = Path.Combine(uploadFolder, storedFileName);

        await using (var stream = new FileStream(physicalPath, FileMode.Create))
        {
            await model.File.CopyToAsync(stream);
        }

        var relativePath = ToStoredFilePath("pvcases", model.PvCaseId.ToString(), storedFileName);
        var currentUserId = _userManager.GetUserId(User) ?? string.Empty;

        var entity = new CaseAttachment
        {
            PvCaseId = model.PvCaseId,
            AttachmentType = model.AttachmentType.Trim(),
            OriginalFileName = storedFileName,
            StoredFileName = storedFileName,
            FilePath = relativePath,
            ContentType = model.File.ContentType,
            FileSizeBytes = model.File.Length,
            Description = model.Description?.Trim(),
            UploadedByUserId = currentUserId,
            UploadedOnUtc = DateTime.UtcNow,
            CreatedBy = currentUserId,
            CreatedOnUtc = DateTime.UtcNow
        };

        _context.CaseAttachments.Add(entity);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Attachment uploaded successfully.";
        return RedirectToAction(nameof(Attachments), new { caseId = model.PvCaseId });
    }
    [HttpGet]
    public async Task<IActionResult> DownloadAttachment(long id)
    {
        var attachment = await _context.CaseAttachments
            .Include(x => x.PvCase)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (attachment == null)
        {
            TempData["ErrorMessage"] = "Attachment not found.";
            return RedirectToAction("Index", "CaseInbox");
        }

        var currentUserId = _userManager.GetUserId(User);

        if (!_caseSecurityService.CanViewCase(attachment.PvCase, User, currentUserId))
        {
            TempData["ErrorMessage"] = "You are not allowed to download this attachment.";
            return RedirectToAction("Index", "CaseInbox");
        }

        var physicalPath = ResolveExistingStoredFilePath(attachment.FilePath);

        if (physicalPath == null)
        {
            TempData["ErrorMessage"] = "Attachment file not found on server.";
            return RedirectToAction(nameof(Attachments), new { caseId = attachment.PvCaseId });
        }

        var bytes = await System.IO.File.ReadAllBytesAsync(physicalPath);

        return File(bytes, attachment.ContentType, attachment.OriginalFileName);
    }

    [HttpGet]
    public async Task<IActionResult> Narrative(long caseId)
    {
        if (!await EnsureCaseViewableAsync(caseId))
        {
            return RedirectToAction("Index", "CaseInbox");
        }

        var pvCase = await GetCaseAsync(caseId);

        if (pvCase == null)
        {
            TempData["ErrorMessage"] = "Case not found.";
            return RedirectToAction("Index", "CaseInbox");
        }

        ViewBag.PvCaseId = pvCase.Id;
        ViewBag.CaseNo = pvCase.CaseNo;
        ViewBag.ActiveTab = "Narrative";
        ViewBag.IsReadOnly = false;

        var model = new CaseNarrativeViewModel
        {
            PvCaseId = pvCase.Id,
            CaseNo = pvCase.CaseNo,
            InitialNarrative = pvCase.Narrative,
            CaseNarrativeHtml = HtmlSanitizer.Sanitize(pvCase.CaseNarrativeHtml),
            IsReadOnly = _caseSecurityService.IsCaseReadOnly(pvCase)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Narrative(CaseNarrativeViewModel model)
    {
        if (!await EnsureCaseEditableAsync(model.PvCaseId))
        {
            return RedirectToAction("Index", "CaseInbox");
        }

        var pvCase = await GetCaseAsync(model.PvCaseId);

        if (pvCase == null)
        {
            TempData["ErrorMessage"] = "Case not found.";
            return RedirectToAction("Index", "CaseInbox");
        }

        var currentUserId = _userManager.GetUserId(User);

        pvCase.CaseNarrativeHtml = HtmlSanitizer.Sanitize(model.CaseNarrativeHtml);
        pvCase.ModifiedBy = currentUserId;
        pvCase.ModifiedOnUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Narrative saved successfully.";

        return RedirectToAction(nameof(Narrative), new { caseId = model.PvCaseId });
    }

    private async Task<PvCase?> GetCaseAsync(long caseId)
    {
        return await _context.PvCases
            .FirstOrDefaultAsync(x => x.Id == caseId && !x.IsDeleted);
    }

    private void UpdateCaseValidity(PvCase pvCase)
    {
        pvCase.IsValidCase =
            pvCase.IsPatientIdentifiable &&
            pvCase.IsReporterIdentifiable &&
            pvCase.IsSuspectProductAvailable &&
            pvCase.IsAdverseEventAvailable;

        if (!pvCase.IsValidCase)
        {
            pvCase.Status = PvCaseStatus.InvalidFollowUpRequired;
        }
        else if (pvCase.Status == PvCaseStatus.InvalidFollowUpRequired ||
                 pvCase.Status == PvCaseStatus.DataEntryInProgress ||
                 pvCase.Status == PvCaseStatus.ValidityPending)
        {
            pvCase.Status = PvCaseStatus.DuplicateCheckPending;
        }

        pvCase.ModifiedOnUtc = DateTime.UtcNow;
    }

    private static DateTime CalculateDueDate(DateTime receiptDate, bool isSerious)
    {
        return isSerious
            ? receiptDate.Date.AddDays(15)
            : receiptDate.Date.AddDays(90);
    }

    private string GetPrivateUploadFolder(params string[] pathParts)
    {
        return Path.Combine(new[] { _environment.ContentRootPath, "App_Data", "uploads" }.Concat(pathParts).ToArray());
    }

    private string? ResolveExistingStoredFilePath(string storedPath)
    {
        var normalizedPath = storedPath
            .TrimStart('/', '\\')
            .Replace("/", Path.DirectorySeparatorChar.ToString())
            .Replace("\\", Path.DirectorySeparatorChar.ToString());

        var candidatePaths = new List<string>
        {
            Path.Combine(_environment.ContentRootPath, normalizedPath)
        };

        if (normalizedPath.StartsWith($"uploads{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
        {
            candidatePaths.Add(Path.Combine(_environment.WebRootPath, normalizedPath));
        }

        foreach (var candidatePath in candidatePaths)
        {
            if (System.IO.File.Exists(candidatePath))
            {
                return candidatePath;
            }
        }

        return null;
    }

    private static string ToStoredFilePath(params string[] pathParts)
    {
        return string.Join("/", new[] { "App_Data", "uploads" }.Concat(pathParts));
    }

    private async Task<string> GenerateAttachmentFileNameAsync(
        long caseId,
        string caseNo,
        string extension,
        string uploadFolder)
    {
        var sequence = await _context.CaseAttachments
            .CountAsync(x => x.PvCaseId == caseId && !x.IsDeleted) + 1;

        var safeCaseNo = SanitizeFileNamePart(caseNo);
        string fileName;

        do
        {
            var prefix = safeCaseNo.StartsWith("InnoPV", StringComparison.OrdinalIgnoreCase)
                ? string.Empty
                : "InnoPV_";

            fileName = $"{prefix}{safeCaseNo}_ATT_{sequence:000}{extension}";
            sequence++;
        }
        while (System.IO.File.Exists(Path.Combine(uploadFolder, fileName)));

        return fileName;
    }

    private static string SanitizeFileNamePart(string value)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var cleaned = new string(value
            .Trim()
            .Select(ch => invalidChars.Contains(ch) || char.IsWhiteSpace(ch) ? '_' : ch)
            .ToArray());

        return string.IsNullOrWhiteSpace(cleaned)
            ? "CASE"
            : cleaned;
    }

    private async Task LoadReporterDropdownsAsync(CaseReporterDetailViewModel model)
    {
        model.ReporterTypeOptions = await GetCommonMasterOptionsAsync(CommonMasterType.ReporterType);
    }

    private async Task LoadAdverseEventDropdownsAsync(CaseAdverseEventDetailViewModel model)
    {
        model.SeriousnessCriteriaOptions = await GetCommonMasterOptionsAsync(CommonMasterType.SeriousnessCriteria);
        model.OutcomeOptions = await GetCommonMasterOptionsAsync(CommonMasterType.Outcome);
    }

    private async Task LoadSuspectProductDropdownsAsync(CaseSuspectProductDetailViewModel model)
    {
        model.ProductOptions = await _context.ProductMasters
            .Where(x => x.IsActive && !x.IsDeleted)
            .OrderBy(x => x.ProductName)
            .Select(x => new SelectListItem
            {
                Value = x.ProductName,
                Text = x.ProductName
            })
            .ToListAsync();

        model.RouteOptions = await GetCommonMasterOptionsAsync(CommonMasterType.Route);
        model.FrequencyOptions = await GetCommonMasterOptionsAsync(CommonMasterType.Frequency);
        model.CausalityOptions = await GetCommonMasterOptionsAsync(CommonMasterType.Causality);
    }

    private async Task LoadMedicationDropdownsAsync(CaseConcomitantMedicationViewModel model)
    {
        model.RouteOptions = await GetCommonMasterOptionsAsync(CommonMasterType.Route);
        model.FrequencyOptions = await GetCommonMasterOptionsAsync(CommonMasterType.Frequency);
    }

    private async Task<List<SelectListItem>> GetCommonMasterOptionsAsync(CommonMasterType masterType)
    {
        return await _context.CommonMasterOptions
            .Where(x => x.MasterType == masterType && x.IsActive && !x.IsDeleted)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .Select(x => new SelectListItem
            {
                Value = x.Name,
                Text = x.Name
            })
            .ToListAsync();
    }
    private async Task<bool> EnsureCaseEditableAsync(long caseId)
    {
        var pvCase = await GetCaseAsync(caseId);

        if (pvCase == null)
        {
            TempData["ErrorMessage"] = "Case not found.";
            return false;
        }

        var currentUserId = _userManager.GetUserId(User);

        if (!_caseSecurityService.CanEditCase(pvCase, User, currentUserId))
        {
            AddSecurityAudit(
                pvCase.Id,
                pvCase.CaseNo,
                "Unauthorized Edit Attempt",
                "User attempted to edit a restricted or closed/finalized case.");

            await _context.SaveChangesAsync();

            TempData["ErrorMessage"] = "You are not allowed to edit this case, or the case is already closed/finalized.";
            return false;
        }

        return true;
    }

    private async Task<bool> EnsureCaseViewableAsync(long caseId)
    {
        var pvCase = await GetCaseAsync(caseId);

        if (pvCase == null)
        {
            TempData["ErrorMessage"] = "Case not found.";
            return false;
        }

        var currentUserId = _userManager.GetUserId(User);

        if (!_caseSecurityService.CanViewCase(pvCase, User, currentUserId))
        {
            AddSecurityAudit(
                pvCase.Id,
                pvCase.CaseNo,
                "Unauthorized View Attempt",
                "User attempted to view a restricted case.");

            await _context.SaveChangesAsync();

            TempData["ErrorMessage"] = "You are not allowed to view this case.";
            return false;
        }

        return true;
    }
    private void AddSecurityAudit(
    long? pvCaseId,
    string? caseNo,
    string actionType,
    string remarks)
    {
        var currentUserId = _userManager.GetUserId(User);
        var currentUserName = User.Identity?.Name;

        var audit = new AuditTrail
        {
            PvCaseId = pvCaseId,
            CaseNo = caseNo,
            EntityName = "Security",
            EntityId = pvCaseId,
            ActionType = actionType,
            FieldName = "Access",
            OldValue = null,
            NewValue = null,
            Remarks = remarks,
            PerformedByUserId = currentUserId,
            PerformedByUserName = currentUserName,
            PerformedOnUtc = DateTime.UtcNow,
            CreatedBy = currentUserId,
            CreatedOnUtc = DateTime.UtcNow
        };

        _context.AuditTrails.Add(audit);
    }


}
