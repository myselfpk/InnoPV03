using InnoPV.Domain.Common;

namespace InnoPV.Domain.Entities;

public class SponsorMaster : BaseEntity
{
    public string SponsorName { get; set; } = string.Empty;
    public string? SponsorCode { get; set; }

    public string? ContactPerson { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }

    public string? Address { get; set; }
    public string? Remarks { get; set; }
}