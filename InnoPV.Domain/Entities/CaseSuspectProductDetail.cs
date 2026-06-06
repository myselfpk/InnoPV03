using InnoPV.Domain.Common;

namespace InnoPV.Domain.Entities;

public class CaseSuspectProductDetail : BaseEntity
{
    public long PvCaseId { get; set; }
    public PvCase PvCase { get; set; } = null!;

    public string ProductName { get; set; } = string.Empty;
    public string? GenericName { get; set; }

    public string? BatchNo { get; set; }
    public DateTime? ExpiryDate { get; set; }

    public string? Dose { get; set; }
    public string? DoseUnit { get; set; }
    public string? Route { get; set; }
    public string? Frequency { get; set; }

    public DateTime? TherapyStartDate { get; set; }
    public DateTime? TherapyStopDate { get; set; }

    public string? Indication { get; set; }

    public string? ActionTaken { get; set; }
    public string? Dechallenge { get; set; }
    public string? Rechallenge { get; set; }

    public string? Causality { get; set; }
    public string? ProductRemarks { get; set; }
}