using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace InnoPV.Web.Models.PvCaseEntry;

public class CaseAttachmentViewModel
{
    public long Id { get; set; }

    public long PvCaseId { get; set; }

    public string? CaseNo { get; set; }

    public string AttachmentType { get; set; } = string.Empty;

    public string OriginalFileName { get; set; } = string.Empty;

    public string StoredFileName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    public string? Description { get; set; }

    public DateTime UploadedOnUtc { get; set; }
}

public class CaseAttachmentUploadViewModel
{
    public long PvCaseId { get; set; }

    public string? CaseNo { get; set; }
    public bool IsReadOnly { get; set; }

    [Required(ErrorMessage = "Attachment type is required.")]
    [StringLength(100)]
    public string AttachmentType { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a file.")]
    public IFormFile? File { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }
}