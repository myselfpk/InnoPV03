using InnoPV.Domain.Common;

namespace InnoPV.Domain.Entities;

public class CaseLabDetail : BaseEntity
{
    public long PvCaseId { get; set; }
    public PvCase PvCase { get; set; } = null!;

    public string TestName { get; set; } = string.Empty;
    public DateTime? TestDate { get; set; }

    public string? TestResult { get; set; }
    public string? Unit { get; set; }
    public string? NormalRange { get; set; }

    public string? ClinicalSignificance { get; set; }
    public string? Remarks { get; set; }
}