namespace file_service.Helpers;

public static class FileValidationHelper
{
    private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg" };
    private static readonly string[] AllowedDocumentExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".csv" };
    private static readonly string[] AllowedCodeExtensions = { ".cs", ".js", ".ts", ".py", ".java", ".cpp", ".c", ".h", ".json", ".xml", ".html", ".css" };
    private static readonly string[] AllowedArchiveExtensions = { ".zip", ".rar", ".7z", ".tar", ".gz" };

    private static readonly string[] AllowedExtensions = AllowedImageExtensions
        .Concat(AllowedDocumentExtensions)
        .Concat(AllowedCodeExtensions)
        .Concat(AllowedArchiveExtensions)
        .ToArray();

    public static bool IsValidFileExtension(string fileName, string[]? allowedExtensions = null)
    {
        var extensionsToCheck = allowedExtensions ?? AllowedExtensions;
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extensionsToCheck.Contains(extension);
    }

    public static bool IsValidFileSize(long fileSize, long maxSizeInBytes = 104857600) // Default 100MB
    {
        return fileSize > 0 && fileSize <= maxSizeInBytes;
    }

    public static bool IsImage(string fileName)
    {
        return IsValidFileExtension(fileName, AllowedImageExtensions);
    }

    public static bool IsDocument(string fileName)
    {
        return IsValidFileExtension(fileName, AllowedDocumentExtensions);
    }

    public static bool IsCodeFile(string fileName)
    {
        return IsValidFileExtension(fileName, AllowedCodeExtensions);
    }

    public static string GetFileCategory(string fileName)
    {
        if (IsImage(fileName)) return "Image";
        if (IsDocument(fileName)) return "Document";
        if (IsCodeFile(fileName)) return "Code";
        if (IsValidFileExtension(fileName, AllowedArchiveExtensions)) return "Archive";
        return "Other";
    }

    public static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }
}
