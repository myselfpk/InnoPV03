using System.ComponentModel.DataAnnotations;

namespace InnoPV.Web.Models.CaseFollowUp;

public class CaseFollowUpCreateViewModel
{
    public long PvCaseId { get; set; }

    public string CaseNo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Receipt date is required.")]
    [DataType(DataType.Date)]
    public DateTime ReceiptDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Source is required.")]
    [StringLength(100)]
    public string Source { get; set; } = string.Empty;

    [StringLength(250)]
    public string? ReceivedFrom { get; set; }

    [Required(ErrorMessage = "Follow-up description is required.")]
    [StringLength(4000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(4000)]
    public string? AdditionalInformation { get; set; }
}