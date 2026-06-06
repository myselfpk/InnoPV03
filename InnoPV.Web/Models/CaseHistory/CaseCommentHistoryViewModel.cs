namespace InnoPV.Web.Models.CaseHistory;

public class CaseCommentHistoryViewModel
{
    public long Id { get; set; }

    public long PvCaseId { get; set; }

    public string FromRole { get; set; } = string.Empty;

    public string ToRole { get; set; } = string.Empty;

    public string CommentType { get; set; } = string.Empty;

    public string CommentText { get; set; } = string.Empty;

    public string? CreatedBy { get; set; }

    public DateTime CreatedOnUtc { get; set; }
}