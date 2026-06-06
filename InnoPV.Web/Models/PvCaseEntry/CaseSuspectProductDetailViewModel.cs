using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace InnoPV.Web.Models.PvCaseEntry;

public class CaseSuspectProductDetailViewModel
{
    public long Id { get; set; }

    public long PvCaseId { get; set; }

    public string? CaseNo { get; set; }
    public bool IsReadOnly { get; set; }

    [Required(ErrorMessage = "Product name is required.")]
    [StringLength(250)]
    public string ProductName { get; set; } = string.Empty;

    [StringLength(250)]
    public string? GenericName { get; set; }

    [StringLength(100)]
    public string? BatchNo { get; set; }

    [DataType(DataType.Date)]
    public DateTime? ExpiryDate { get; set; }

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

    [StringLength(150)]
    public string? ActionTaken { get; set; }

    [StringLength(150)]
    public string? Dechallenge { get; set; }

    [StringLength(150)]
    public string? Rechallenge { get; set; }

    [StringLength(150)]
    public string? Causality { get; set; }

    [StringLength(2000)]
    public string? ProductRemarks { get; set; }

    public List<SelectListItem> ProductOptions { get; set; } = new();

    public List<SelectListItem> RouteOptions { get; set; } = new();

    public List<SelectListItem> FrequencyOptions { get; set; } = new();

    public List<SelectListItem> CausalityOptions { get; set; } = new();
}