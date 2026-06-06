using InnoPV.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace InnoPV.Web.Models.CaseCompleteReport;

public class CaseCompleteReportFilterViewModel
{
    public string? CaseNo { get; set; }

    public string? ProductName { get; set; }

    public string? EventTerm { get; set; }

    public PvCaseStatus? Status { get; set; }

    public bool? IsSerious { get; set; }

    public bool? IsValidCase { get; set; }

    [DataType(DataType.Date)]
    public DateTime? ReceiptDateFrom { get; set; }

    [DataType(DataType.Date)]
    public DateTime? ReceiptDateTo { get; set; }

    public List<SelectListItem> StatusOptions { get; set; } = new();
}