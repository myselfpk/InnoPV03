using InnoPV.Domain.Enums;

namespace InnoPV.Web.Models.ClosureValidation;

public class CaseClosureValidationViewModel
{
    public long PvCaseId { get; set; }

    public string CaseNo { get; set; } = string.Empty;

    public PvCaseStatus Status { get; set; }

    public bool CanClose
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

    public List<CaseClosureValidationItemViewModel> RequiredChecks { get; set; } = new();

    public List<CaseClosureValidationItemViewModel> WarningChecks { get; set; } = new();
}