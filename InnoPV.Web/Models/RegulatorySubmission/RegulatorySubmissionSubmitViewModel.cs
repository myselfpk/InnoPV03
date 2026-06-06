using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace InnoPV.Web.Models.RegulatorySubmission;

public class RegulatorySubmissionSubmitViewModel
{
    public long SubmissionId { get; set; }

    public long PvCaseId { get; set; }

    public string SubmissionNo { get; set; } = string.Empty;

    public string CaseNo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Submitted date is required.")]
    [DataType(DataType.Date)]
    public DateTime SubmittedDate { get; set; } = DateTime.Today;

    [StringLength(200)]
    public string? ReferenceNo { get; set; }

    [StringLength(4000)]
    public string? Remarks { get; set; }

    public bool IsAcknowledgementExpected { get; set; } = true;

    public IFormFile? SubmissionDocument { get; set; }
}