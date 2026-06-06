using InnoPV.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace InnoPV.Web.Models.RegulatorySubmissionReport;

public class RegulatorySubmissionReportFilterViewModel
{
    public string? CaseNo { get; set; }

    public string? SubmissionNo { get; set; }

    public string? SubmissionType { get; set; }

    public string? RecipientAuthority { get; set; }

    public string? SubmissionFormat { get; set; }

    public RegulatorySubmissionStatus? SubmissionStatus { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DueDateFrom { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DueDateTo { get; set; }

    [DataType(DataType.Date)]
    public DateTime? SubmittedDateFrom { get; set; }

    [DataType(DataType.Date)]
    public DateTime? SubmittedDateTo { get; set; }

    public bool? IsOverdue { get; set; }

    public List<SelectListItem> SubmissionStatusOptions { get; set; } = new();

    public List<SelectListItem> SubmissionTypeOptions { get; set; } = new();

    public List<SelectListItem> RecipientAuthorityOptions { get; set; } = new();

    public List<SelectListItem> SubmissionFormatOptions { get; set; } = new();
}