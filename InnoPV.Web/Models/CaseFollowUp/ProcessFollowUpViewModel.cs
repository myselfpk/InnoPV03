using System.ComponentModel.DataAnnotations;

namespace InnoPV.Web.Models.CaseFollowUp;

public class ProcessFollowUpViewModel
{
    public long FollowUpId { get; set; }

    public long PvCaseId { get; set; }

    [Required(ErrorMessage = "Processed remarks are required.")]
    [StringLength(4000)]
    public string ProcessedRemarks { get; set; } = string.Empty;
}