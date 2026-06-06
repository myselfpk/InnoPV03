using InnoPV.Domain.Enums;

namespace InnoPV.Web.Models.Dashboard;

public class SlaCaseItemViewModel
{
    public long Id { get; set; }

    public string CaseNo { get; set; } = string.Empty;

    public string? ProductName { get; set; }

    public string? EventTerm { get; set; }

    public bool IsSerious { get; set; }

    public DateTime ReceiptDate { get; set; }

    public DateTime? DueDate { get; set; }

    public PvCaseStatus Status { get; set; }

    public string? CurrentAssignedRole { get; set; }

    public string? CurrentAssignedUserName { get; set; }

    public int DaysRemaining
    {
        get
        {
            if (!DueDate.HasValue)
            {
                return 0;
            }

            return (DueDate.Value.Date - DateTime.UtcNow.Date).Days;
        }
    }

    public bool IsOverdue
    {
        get
        {
            return DueDate.HasValue &&
                   DueDate.Value.Date < DateTime.UtcNow.Date &&
                   Status != PvCaseStatus.CaseClosed &&
                   Status != PvCaseStatus.CaseFinalized &&
                   Status != PvCaseStatus.Submitted;
        }
    }
}