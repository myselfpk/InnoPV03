using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace InnoPV.Web.Models.RegulatorySubmission;

public class RegulatorySubmissionCreateViewModel
{
    public long PvCaseId { get; set; }

    public string CaseNo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Submission type is required.")]
    [StringLength(100)]
    public string SubmissionType { get; set; } = string.Empty;

    [Required(ErrorMessage = "Recipient authority is required.")]
    [StringLength(200)]
    public string RecipientAuthority { get; set; } = string.Empty;

    [Required(ErrorMessage = "Submission format is required.")]
    [StringLength(100)]
    public string SubmissionFormat { get; set; } = string.Empty;

    [Required(ErrorMessage = "Due date is required.")]
    [DataType(DataType.Date)]
    public DateTime DueDate { get; set; } = DateTime.Today;

    [StringLength(4000)]
    public string? Remarks { get; set; }

    public IFormFile? SubmissionDocument { get; set; }
}