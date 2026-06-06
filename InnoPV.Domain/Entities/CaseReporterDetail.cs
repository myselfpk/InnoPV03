using InnoPV.Domain.Common;

namespace InnoPV.Domain.Entities;

public class CaseReporterDetail : BaseEntity
{
    public long PvCaseId { get; set; }
    public PvCase PvCase { get; set; } = null!;

    public string ReporterName { get; set; } = string.Empty;
    public string? ReporterType { get; set; }

    public string? Qualification { get; set; }
    public string? OrganizationName { get; set; }
    public string? Department { get; set; }

    public string? Email { get; set; }
    public string? Phone { get; set; }

    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }

    public DateTime? DateOfReport { get; set; }

    public bool ConsentForFollowUp { get; set; }
    public string? ReporterRemarks { get; set; }
}