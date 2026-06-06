using System.ComponentModel.DataAnnotations;

namespace InnoPV.Web.Models.DuplicateCheck;

public class MarkNotDuplicateViewModel
{
    public long CaseId { get; set; }

    [Required(ErrorMessage = "Remarks are required.")]
    [StringLength(2000)]
    public string Remarks { get; set; } = string.Empty;
}