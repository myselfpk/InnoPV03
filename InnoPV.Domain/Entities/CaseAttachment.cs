using InnoPV.Domain.Common;

namespace InnoPV.Domain.Entities;

public class CaseAttachment : BaseEntity
{
    public long PvCaseId { get; set; }
    public PvCase PvCase { get; set; } = null!;

    public string AttachmentType { get; set; } = string.Empty;

    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }

    public string? Description { get; set; }

    public string UploadedByUserId { get; set; } = string.Empty;
    public DateTime UploadedOnUtc { get; set; } = DateTime.UtcNow;
}