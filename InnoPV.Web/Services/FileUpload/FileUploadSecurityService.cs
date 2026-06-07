namespace InnoPV.Web.Services.FileUpload;

public class FileUploadSecurityService : IFileUploadSecurityService
{
    private static readonly IReadOnlyCollection<string> DefaultAllowedExtensions =
        new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx", ".xls", ".xlsx" };

    private readonly IWebHostEnvironment _environment;

    public FileUploadSecurityService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<FileUploadValidationResult> ValidateDocumentAsync(
        IFormFile? file,
        long maxFileSizeBytes,
        IReadOnlyCollection<string>? allowedExtensions = null)
    {
        if (file == null || file.Length == 0)
        {
            return FileUploadValidationResult.Failure("Please select a valid file.");
        }

        var originalFileName = Path.GetFileName(file.FileName);

        if (string.IsNullOrWhiteSpace(originalFileName))
        {
            return FileUploadValidationResult.Failure("Invalid file name.");
        }

        var blockedFileNameChars = Path.GetInvalidFileNameChars();

        if (originalFileName.Any(ch => blockedFileNameChars.Contains(ch)))
        {
            return FileUploadValidationResult.Failure("File name contains invalid characters.");
        }

        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(extension))
        {
            return FileUploadValidationResult.Failure("File extension is required.");
        }

        var extensions = allowedExtensions ?? DefaultAllowedExtensions;

        if (!extensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            return FileUploadValidationResult.Failure("Only PDF, image, Word and Excel files are allowed.");
        }

        if (!await IsAllowedDocumentContentAsync(file, extension))
        {
            return FileUploadValidationResult.Failure("File content does not match the selected file type.");
        }

        if (file.Length > maxFileSizeBytes)
        {
            return FileUploadValidationResult.Failure($"Maximum file size allowed is {maxFileSizeBytes / (1024 * 1024)} MB.");
        }

        return FileUploadValidationResult.Success(
            originalFileName,
            extension,
            GetDocumentContentType(extension));
    }

    public string GetPrivateUploadFolder(params string[] pathParts)
    {
        return Path.Combine(new[] { _environment.ContentRootPath, "App_Data", "uploads" }.Concat(pathParts).ToArray());
    }

    public string ToStoredFilePath(params string[] pathParts)
    {
        return string.Join("/", new[] { "App_Data", "uploads" }.Concat(pathParts));
    }

    public string? ResolvePrivateUploadPath(string storedPath, params string[] uploadRootPathParts)
    {
        var uploadRoot = Path.GetFullPath(GetPrivateUploadFolder(uploadRootPathParts));
        var normalizedPath = storedPath
            .TrimStart('/', '\\')
            .Replace("/", Path.DirectorySeparatorChar.ToString())
            .Replace("\\", Path.DirectorySeparatorChar.ToString());

        var candidatePaths = new List<string>
        {
            Path.Combine(_environment.ContentRootPath, normalizedPath)
        };

        if (!string.IsNullOrWhiteSpace(_environment.WebRootPath)
            && normalizedPath.StartsWith($"uploads{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
        {
            candidatePaths.Add(Path.Combine(_environment.WebRootPath, normalizedPath));
        }

        foreach (var candidatePath in candidatePaths)
        {
            var fullPath = Path.GetFullPath(candidatePath);

            if (IsPathInsideDirectory(fullPath, uploadRoot))
            {
                return fullPath;
            }
        }

        return null;
    }

    private static bool IsPathInsideDirectory(string path, string directory)
    {
        var normalizedDirectory = directory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;

        return path.StartsWith(normalizedDirectory, StringComparison.OrdinalIgnoreCase);
    }

    private static string GetDocumentContentType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            _ => "application/octet-stream"
        };
    }

    private static async Task<bool> IsAllowedDocumentContentAsync(IFormFile file, string extension)
    {
        var header = new byte[8];
        await using var stream = file.OpenReadStream();
        var bytesRead = await stream.ReadAsync(header);

        return extension.ToLowerInvariant() switch
        {
            ".pdf" => StartsWith(header, bytesRead, 0x25, 0x50, 0x44, 0x46),
            ".jpg" or ".jpeg" => StartsWith(header, bytesRead, 0xFF, 0xD8, 0xFF),
            ".png" => StartsWith(header, bytesRead, 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A),
            ".doc" or ".xls" => StartsWith(header, bytesRead, 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1),
            ".docx" or ".xlsx" => StartsWith(header, bytesRead, 0x50, 0x4B, 0x03, 0x04)
                || StartsWith(header, bytesRead, 0x50, 0x4B, 0x05, 0x06)
                || StartsWith(header, bytesRead, 0x50, 0x4B, 0x07, 0x08),
            _ => false
        };
    }

    private static bool StartsWith(byte[] source, int length, params byte[] expected)
    {
        if (length < expected.Length)
        {
            return false;
        }

        for (var i = 0; i < expected.Length; i++)
        {
            if (source[i] != expected[i])
            {
                return false;
            }
        }

        return true;
    }
}
