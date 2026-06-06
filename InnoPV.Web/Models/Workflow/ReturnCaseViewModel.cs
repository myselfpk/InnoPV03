using System.ComponentModel.DataAnnotations;

namespace InnoPV.Web.Models.Workflow;

public class ReturnCaseViewModel
{
    public long PvCaseId { get; set; }

    public string CaseNo { get; set; } = string.Empty;

    public string FromRole { get; set; } = string.Empty;

    public string ToRole { get; set; } = string.Empty;

    [Required(ErrorMessage = "Return comment is required.")]
    [StringLength(2000)]
    public string CommentText { get; set; } = string.Empty;
}