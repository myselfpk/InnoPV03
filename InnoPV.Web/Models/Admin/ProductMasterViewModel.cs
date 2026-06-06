using System.ComponentModel.DataAnnotations;

namespace InnoPV.Web.Models.Admin;

public class ProductMasterViewModel
{
    public long Id { get; set; }

    [Required(ErrorMessage = "Product name is required.")]
    [StringLength(250)]
    public string ProductName { get; set; } = string.Empty;

    [StringLength(250)]
    public string? GenericName { get; set; }

    [StringLength(50)]
    public string? ProductCode { get; set; }

    [StringLength(100)]
    public string? ProductType { get; set; }

    [StringLength(100)]
    public string? Strength { get; set; }

    [StringLength(100)]
    public string? DosageForm { get; set; }

    [StringLength(250)]
    public string? ManufacturerName { get; set; }

    [StringLength(250)]
    public string? MarketingAuthorizationHolder { get; set; }

    [StringLength(1000)]
    public string? Remarks { get; set; }

    public bool IsActive { get; set; } = true;
}