using InnoPV.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace InnoPV.Web.Models.Reports;

public class CaseReportFilterViewModel
{
    public string? CaseNo { get; set; }

    [DataType(DataType.Date)]
    public DateTime? ReceiptDateFrom { get; set; }

    [DataType(DataType.Date)]
    public DateTime? ReceiptDateTo { get; set; }

    public PvCaseStatus? Status { get; set; }

    public string? AssignedRole { get; set; }

    public string? ProductName { get; set; }

    public bool? IsSerious { get; set; }

    public bool? IsValidCase { get; set; }

    public List<SelectListItem> StatusOptions { get; set; } = new();

    public List<SelectListItem> RoleOptions { get; set; } = new();

    public List<SelectListItem> ProductOptions { get; set; } = new();
}