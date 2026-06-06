using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace InnoPV.Web.Models.PvCaseEntry;

public class CaseAdverseEventDetailViewModel
{
    public long Id { get; set; }

    public long PvCaseId { get; set; }

    public string? CaseNo { get; set; }
    public bool IsReadOnly { get; set; }

    [Required(ErrorMessage = "Event term is required.")]
    [StringLength(250)]
    public string EventTerm { get; set; } = string.Empty;

    [StringLength(3000)]
    public string? EventDescription { get; set; }

    [DataType(DataType.Date)]
    public DateTime? EventStartDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? EventStopDate { get; set; }

    public bool IsSerious { get; set; }

    [StringLength(250)]
    public string? SeriousnessCriteria { get; set; }

    [StringLength(100)]
    public string? Severity { get; set; }

    [StringLength(100)]
    public string? Outcome { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DeathDate { get; set; }

    [StringLength(1000)]
    public string? CauseOfDeath { get; set; }

    public bool WasHospitalized { get; set; }

    [DataType(DataType.Date)]
    public DateTime? HospitalizationDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DischargeDate { get; set; }

    [StringLength(2000)]
    public string? TreatmentGivenForEvent { get; set; }

    [StringLength(2000)]
    public string? EventRemarks { get; set; }

    public List<SelectListItem> SeriousnessCriteriaOptions { get; set; } = new();

    public List<SelectListItem> OutcomeOptions { get; set; } = new();
}