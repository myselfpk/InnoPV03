using InnoPV.Domain.Common;

namespace InnoPV.Domain.Entities;

public class CaseComment : BaseEntity
{
    public long PvCaseId { get; set; }
    public PvCase PvCase { get; set; } = null!;

    public string FromRole { get; set; } = string.Empty;
    public string ToRole { get; set; } = string.Empty;

    public string CommentType { get; set; } = "General";
    public string CommentText { get; set; } = string.Empty;

    public string CommentedByUserId { get; set; } = string.Empty;
    public string CommentedByRole { get; set; } = string.Empty;

    public bool IsCorrectionRequired { get; set; }
    public bool IsResolved { get; set; }
    public DateTime CommentedOnUtc { get; set; } = DateTime.UtcNow;
}