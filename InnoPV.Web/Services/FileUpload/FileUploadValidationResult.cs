namespace InnoPV.Web.Services.FileUpload;

public class FileUploadValidationResult
{
    private FileUploadValidationResult(
        bool isValid,
        string? errorMessage,
        string originalFileName,
        string extension,
        string contentType)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
        OriginalFileName = originalFileName;
        Extension = extension;
        ContentType = contentType;
    }

    public bool IsValid { get; }
    public string? ErrorMessage { get; }
    public string OriginalFileName { get; }
    public string Extension { get; }
    public string ContentType { get; }

    public static FileUploadValidationResult Success(
        string originalFileName,
        string extension,
        string contentType)
    {
        return new FileUploadValidationResult(true, null, originalFileName, extension, contentType);
    }

    public static FileUploadValidationResult Failure(string errorMessage)
    {
        return new FileUploadValidationResult(false, errorMessage, string.Empty, string.Empty, string.Empty);
    }
}
