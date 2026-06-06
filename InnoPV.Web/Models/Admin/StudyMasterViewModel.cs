using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace InnoPV.Web.Models.Admin;

public class StudyMasterViewModel
{
    public long Id { get; set; }

    [Required(ErrorMessage = "Study title is required.")]
    [StringLength(500)]
    public string StudyTitle { get; set; } = string.Empty;

    [StringLength(150)]
    public string? ProtocolNo { get; set; }

    [StringLength(100)]
    public string? StudyCode { get; set; }

    public long? SponsorMasterId { get; set; }

    public string? SponsorName { get; set; }

    [StringLength(250)]
    public string? Indication { get; set; }

    [StringLength(150)]
    public string? StudyType { get; set; }

    [StringLength(1000)]
    public string? Remarks { get; set; }

    public bool IsActive { get; set; } = true;

    public List<SelectListItem> SponsorOptions { get; set; } = new();
}