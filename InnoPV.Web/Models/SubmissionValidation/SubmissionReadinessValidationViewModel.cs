using InnoPV.Domain.Enums;

namespace InnoPV.Web.Models.SubmissionValidation;

public class SubmissionReadinessValidationViewModel
{
    public long PvCaseId { get; set; }

    public string CaseNo { get; set; } = string.Empty;

    public PvCaseStatus Status { get; set; }

    public DateTime GeneratedOnUtc { get; set; } = DateTime.UtcNow;

    public bool CanSubmit
    {
        get
        {
            return RequiredChecks.All(x => x.IsPassed);
        }
    }

    public int RequiredFailedCount
    {
        get
        {
            return RequiredChecks.Count(x => !x.IsPassed);
        }
    }

    public int WarningCount
    {
        get
        {
            return WarningChecks.Count(x => !x.IsPassed);
        }
    }

    public List<SubmissionReadinessCheckItemViewModel> RequiredChecks { get; set; } = new();

    public List<SubmissionReadinessCheckItemViewModel> WarningChecks { get; set; } = new();
}
