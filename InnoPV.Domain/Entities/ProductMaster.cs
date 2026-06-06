using InnoPV.Domain.Common;

namespace InnoPV.Domain.Entities;

public class ProductMaster : BaseEntity
{
    public string ProductName { get; set; } = string.Empty;
    public string? GenericName { get; set; }
    public string? ProductCode { get; set; }

    public string? ProductType { get; set; }
    public string? Strength { get; set; }
    public string? DosageForm { get; set; }

    public string? ManufacturerName { get; set; }
    public string? MarketingAuthorizationHolder { get; set; }

    public string? Remarks { get; set; }
}