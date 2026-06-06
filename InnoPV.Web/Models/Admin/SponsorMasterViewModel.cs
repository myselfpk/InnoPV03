using System.ComponentModel.DataAnnotations;

namespace InnoPV.Web.Models.Admin;

public class SponsorMasterViewModel
{
    public long Id { get; set; }

    [Required(ErrorMessage = "Sponsor name is required.")]
    [StringLength(250)]
    public string SponsorName { get; set; } = string.Empty;

    [StringLength(50)]
    public string? SponsorCode { get; set; }

    [StringLength(150)]
    public string? ContactPerson { get; set; }

    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [StringLength(150)]
    public string? ContactEmail { get; set; }

    [StringLength(30)]
    public string? ContactPhone { get; set; }

    [StringLength(1000)]
    public string? Address { get; set; }

    [StringLength(1000)]
    public string? Remarks { get; set; }

    public bool IsActive { get; set; } = true;
}