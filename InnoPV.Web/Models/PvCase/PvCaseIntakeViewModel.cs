using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace InnoPV.Web.Models.PvCase;

public class PvCaseIntakeViewModel
{
    public long Id { get; set; }

    public string? CaseNo { get; set; }

    [Required(ErrorMessage = "Case source is required.")]
    public string CaseSource { get; set; } = string.Empty;

    [Required(ErrorMessage = "Receipt date is required.")]
    [DataType(DataType.Date)]
    public DateTime ReceiptDate { get; set; } = DateTime.Today;

    [StringLength(200)]
    public string? InitialReporterName { get; set; }

    [StringLength(100)]
    public string? InitialPatientIdentifier { get; set; }

    [StringLength(200)]
    public string? InitialProductName { get; set; }

    [StringLength(250)]
    public string? InitialEventTerm { get; set; }

    public bool IsPatientIdentifiable { get; set; }

    public bool IsReporterIdentifiable { get; set; }

    public bool IsSuspectProductAvailable { get; set; }

    public bool IsAdverseEventAvailable { get; set; }

    public bool IsSerious { get; set; }

    [StringLength(2000)]
    public string? Narrative { get; set; }

    [StringLength(1000)]
    public string? Remarks { get; set; }

    public int CompletenessScore { get; set; }

    public List<SelectListItem> CaseSourceOptions { get; set; } = new();

    public List<SelectListItem> ProductOptions { get; set; } = new();
}