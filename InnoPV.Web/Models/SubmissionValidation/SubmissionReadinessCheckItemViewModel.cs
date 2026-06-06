namespace InnoPV.Web.Models.SubmissionValidation;

public class SubmissionReadinessCheckItemViewModel
{
    public string SectionName { get; set; } = string.Empty;

    public string CheckDescription { get; set; } = string.Empty;

    public bool IsPassed { get; set; }

    public string Message { get; set; } = string.Empty;
}
