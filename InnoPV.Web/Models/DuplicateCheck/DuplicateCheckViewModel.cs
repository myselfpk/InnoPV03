using InnoPV.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace InnoPV.Web.Models.DuplicateCheck;

public class DuplicateCheckViewModel
{
    public long PvCaseId { get; set; }

    public string CaseNo { get; set; } = string.Empty;

    public string? PatientIdentifier { get; set; }

    public string? ReporterName { get; set; }

    public string? ProductName { get; set; }

    public string? EventTerm { get; set; }

    public DateTime ReceiptDate { get; set; }

    public PvCaseStatus Status { get; set; }

    public int DuplicateConfidenceThreshold { get; set; } = 55;

    [Required(ErrorMessage = "Remarks are required.")]
    [StringLength(2000)]
    public string NotDuplicateRemarks { get; set; } = string.Empty;

    public List<DuplicateCandidateViewModel> Candidates { get; set; } = new();
}