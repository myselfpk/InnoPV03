using System.ComponentModel.DataAnnotations;

namespace InnoPV.Web.Models.PvCaseEntry;

public class CaseLabDetailViewModel
{
    public long Id { get; set; }

    public long PvCaseId { get; set; }

    public string? CaseNo { get; set; }
    public bool IsReadOnly { get; set; }

    [Required(ErrorMessage = "Test name is required.")]
    [StringLength(250)]
    public string TestName { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime? TestDate { get; set; }

    [StringLength(250)]
    public string? TestResult { get; set; }

    [StringLength(50)]
    public string? Unit { get; set; }

    [StringLength(100)]
    public string? NormalRange { get; set; }

    [StringLength(1000)]
    public string? ClinicalSignificance { get; set; }

    [StringLength(1000)]
    public string? Remarks { get; set; }

    public bool IsActive { get; set; } = true;
}