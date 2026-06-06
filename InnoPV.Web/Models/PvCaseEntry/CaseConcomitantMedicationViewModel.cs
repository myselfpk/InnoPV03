using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace InnoPV.Web.Models.PvCaseEntry;

public class CaseConcomitantMedicationViewModel
{
    public long Id { get; set; }

    public long PvCaseId { get; set; }

    public string? CaseNo { get; set; }
    public bool IsReadOnly { get; set; }

    [Required(ErrorMessage = "Medication name is required.")]
    [StringLength(250)]
    public string MedicationName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Dose { get; set; }

    [StringLength(50)]
    public string? DoseUnit { get; set; }

    [StringLength(100)]
    public string? Route { get; set; }

    [StringLength(100)]
    public string? Frequency { get; set; }

    [DataType(DataType.Date)]
    public DateTime? TherapyStartDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? TherapyStopDate { get; set; }

    [StringLength(250)]
    public string? Indication { get; set; }

    public bool IsMedicationForEventTreatment { get; set; }

    [StringLength(1000)]
    public string? Remarks { get; set; }

    public bool IsActive { get; set; } = true;

    public List<SelectListItem> RouteOptions { get; set; } = new();

    public List<SelectListItem> FrequencyOptions { get; set; } = new();
}