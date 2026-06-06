using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace InnoPV.Web.Models.PvCaseEntry;

public class CaseReporterDetailViewModel
{
    public long Id { get; set; }

    public long PvCaseId { get; set; }

    public string? CaseNo { get; set; }
    public bool IsReadOnly { get; set; }

    [Required(ErrorMessage = "Reporter name is required.")]
    [StringLength(200)]
    public string ReporterName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? ReporterType { get; set; }

    [StringLength(100)]
    public string? Qualification { get; set; }

    [StringLength(250)]
    public string? OrganizationName { get; set; }

    [StringLength(150)]
    public string? Department { get; set; }

    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [StringLength(150)]
    public string? Email { get; set; }

    [StringLength(30)]
    public string? Phone { get; set; }

    [StringLength(1000)]
    public string? Address { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(100)]
    public string? State { get; set; }

    [StringLength(100)]
    public string? Country { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DateOfReport { get; set; }

    public bool ConsentForFollowUp { get; set; }

    [StringLength(1000)]
    public string? ReporterRemarks { get; set; }

    public List<SelectListItem> ReporterTypeOptions { get; set; } = new();
}