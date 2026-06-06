using InnoPV.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace InnoPV.Web.Models.PvCase;

public class PvCaseEditViewModel : PvCaseIntakeViewModel
{
    public PvCaseStatus Status { get; set; }

    public DateTime? DueDate { get; set; }

    [Required(ErrorMessage = "Reason for change is required.")]
    [StringLength(1000)]
    public string ChangeReason { get; set; } = string.Empty;
}