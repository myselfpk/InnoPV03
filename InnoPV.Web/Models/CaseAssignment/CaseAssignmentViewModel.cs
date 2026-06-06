using InnoPV.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace InnoPV.Web.Models.CaseAssignment;

public class CaseAssignmentViewModel
{
    public long PvCaseId { get; set; }

    public string CaseNo { get; set; } = string.Empty;

    public PvCaseStatus Status { get; set; }

    public string? CurrentAssignedRole { get; set; }

    public string? CurrentAssignedUserId { get; set; }

    public string? CurrentAssignedUserName { get; set; }

    [Required(ErrorMessage = "Assigned role is required.")]
    public string AssignToRole { get; set; } = string.Empty;

    [Required(ErrorMessage = "Assigned user is required.")]
    public string AssignToUserId { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Remarks { get; set; }

    public List<SelectListItem> RoleOptions { get; set; } = new();

    public List<SelectListItem> UserOptions { get; set; } = new();
}