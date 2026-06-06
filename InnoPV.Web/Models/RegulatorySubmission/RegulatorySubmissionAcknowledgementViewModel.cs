using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace InnoPV.Web.Models.RegulatorySubmission;

public class RegulatorySubmissionAcknowledgementViewModel
{
    public long SubmissionId { get; set; }

    public long PvCaseId { get; set; }

    public string SubmissionNo { get; set; } = string.Empty;

    public string CaseNo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Acknowledgement received date is required.")]
    [DataType(DataType.Date)]
    public DateTime AcknowledgementReceivedDate { get; set; } = DateTime.Today;

    [StringLength(200)]
    public string? ReferenceNo { get; set; }

    [StringLength(4000)]
    public string? AcknowledgementRemarks { get; set; }

    public IFormFile? AcknowledgementDocument { get; set; }
}