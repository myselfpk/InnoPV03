namespace InnoPV.Web.Services.FileUpload;

public interface IFileUploadSecurityService
{
    Task<FileUploadValidationResult> ValidateDocumentAsync(
        IFormFile? file,
        long maxFileSizeBytes,
        IReadOnlyCollection<string>? allowedExtensions = null);

    string GetPrivateUploadFolder(params string[] pathParts);

    string ToStoredFilePath(params string[] pathParts);

    string? ResolvePrivateUploadPath(string storedPath, params string[] uploadRootPathParts);
}
