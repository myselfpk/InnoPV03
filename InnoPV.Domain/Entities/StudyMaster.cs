using InnoPV.Domain.Common;

namespace InnoPV.Domain.Entities;

public class StudyMaster : BaseEntity
{
    public string StudyTitle { get; set; } = string.Empty;
    public string? ProtocolNo { get; set; }
    public string? StudyCode { get; set; }

    public long? SponsorMasterId { get; set; }
    public SponsorMaster? SponsorMaster { get; set; }

    public string? Indication { get; set; }
    public string? StudyType { get; set; }

    public string? Remarks { get; set; }
}