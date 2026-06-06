using InnoPV.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace InnoPV.Web.Models.Workflow;

public class CaseChecklistCaptureViewModel
{
    public long PvCaseId { get; set; }

    public string CaseNo { get; set; } = string.Empty;

    public PvCaseStatus CurrentStatus { get; set; }

    public string ApplicableRole { get; set; } = string.Empty;

    public string ApplicableStage { get; set; } = string.Empty;

    public long ChecklistMasterId { get; set; }

    public string ChecklistName { get; set; } = string.Empty;

    public List<CaseChecklistItemCaptureViewModel> Items { get; set; } = new();

    [StringLength(2000)]
    public string? WorkflowComment { get; set; }
}