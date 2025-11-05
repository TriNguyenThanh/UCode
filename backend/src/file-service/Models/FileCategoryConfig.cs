using file_service.Enums;

namespace file_service.Models;

public class FileCategoryConfig
{
    public FileCategory Category { get; set; }
    public string FolderPath { get; set; } = string.Empty;
    public long MaxFileSizeBytes { get; set; }
    public List<string> AllowedExtensions { get; set; } = new();
    public List<string> AllowedMimeTypes { get; set; } = new();
}

public static class FileCategoryConfiguration
{
    private static readonly Dictionary<FileCategory, FileCategoryConfig> _configurations = new()
    {
        [FileCategory.AssignmentDocument] = new FileCategoryConfig
        {
            Category = FileCategory.AssignmentDocument,
            FolderPath = "assignments",
            MaxFileSizeBytes = 20 * 1024 * 1024, // 20MB
            AllowedExtensions = new List<string> { ".pdf", ".docx", ".doc", ".txt", ".md" },
            AllowedMimeTypes = new List<string>
            {
                "application/pdf",
                "application/msword",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "text/plain",
                "text/markdown"
            }
        },
        [FileCategory.CodeSubmission] = new FileCategoryConfig
        {
            Category = FileCategory.CodeSubmission,
            FolderPath = "submissions",
            MaxFileSizeBytes = 5 * 1024 * 1024, // 5MB
            AllowedExtensions = new List<string>
            {
                ".zip", ".rar", ".7z",
                ".c", ".cpp", ".h", ".hpp",
                ".java", ".class",
                ".py", ".pyc",
                ".js", ".ts",
                ".cs",
                ".go",
                ".rb",
                ".php",
                ".txt"
            },
            AllowedMimeTypes = new List<string>
            {
                "application/zip",
                "application/x-zip-compressed",
                "application/x-rar-compressed",
                "application/x-7z-compressed",
                "text/plain",
                "text/x-c",
                "text/x-c++",
                "text/x-java",
                "text/x-python",
                "application/javascript",
                "application/typescript",
                "text/x-csharp",
                "application/octet-stream"
            }
        },
        [FileCategory.Image] = new FileCategoryConfig
        {
            Category = FileCategory.Image,
            FolderPath = "images",
            MaxFileSizeBytes = 10 * 1024 * 1024, // 10MB
            AllowedExtensions = new List<string> { ".jpg", ".jpeg", ".png", ".gif", ".svg", ".webp" },
            AllowedMimeTypes = new List<string>
            {
                "image/jpeg",
                "image/png",
                "image/gif",
                "image/svg+xml",
                "image/webp"
            }
        },
        [FileCategory.Avatar] = new FileCategoryConfig
        {
            Category = FileCategory.Avatar,
            FolderPath = "avatars",
            MaxFileSizeBytes = 2 * 1024 * 1024, // 2MB
            AllowedExtensions = new List<string> { ".jpg", ".jpeg", ".png", ".webp" },
            AllowedMimeTypes = new List<string>
            {
                "image/jpeg",
                "image/png",
                "image/webp"
            }
        },
        [FileCategory.TestCase] = new FileCategoryConfig
        {
            Category = FileCategory.TestCase,
            FolderPath = "testcases",
            MaxFileSizeBytes = 2 * 1024 * 1024, // 2MB
            AllowedExtensions = new List<string> { ".txt", ".in", ".out" },
            AllowedMimeTypes = new List<string>
            {
                "text/plain"
            }
        },
        [FileCategory.Reference] = new FileCategoryConfig
        {
            Category = FileCategory.Reference,
            FolderPath = "references",
            MaxFileSizeBytes = 20 * 1024 * 1024, // 20MB
            AllowedExtensions = new List<string> { ".pdf", ".docx", ".doc", ".pptx", ".ppt", ".xlsx", ".xls" },
            AllowedMimeTypes = new List<string>
            {
                "application/pdf",
                "application/msword",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "application/vnd.ms-powerpoint",
                "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                "application/vnd.ms-excel",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            }
        },
        [FileCategory.Document] = new FileCategoryConfig
        {
            Category = FileCategory.Document,
            FolderPath = "documents",
            MaxFileSizeBytes = 50 * 1024 * 1024, // 50MB
            AllowedExtensions = new List<string> 
            { 
                // PDF
                ".pdf",
                // Word
                ".doc", ".docx", ".dot", ".dotx", ".docm",
                // Excel
                ".xls", ".xlsx", ".xlsm", ".xlt", ".xltx", ".xlsb",
                // PowerPoint
                ".ppt", ".pptx", ".pptm", ".pps", ".ppsx",
                // OpenDocument
                ".odt", ".ods", ".odp",
                // Rich Text
                ".rtf",
                // Text
                ".txt", ".md"
            },
            AllowedMimeTypes = new List<string>
            {
                // PDF
                "application/pdf",
                // Word
                "application/msword",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "application/vnd.ms-word.document.macroEnabled.12",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.template",
                // Excel
                "application/vnd.ms-excel",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "application/vnd.ms-excel.sheet.macroEnabled.12",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.template",
                "application/vnd.ms-excel.sheet.binary.macroEnabled.12",
                // PowerPoint
                "application/vnd.ms-powerpoint",
                "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                "application/vnd.ms-powerpoint.presentation.macroEnabled.12",
                "application/vnd.ms-powerpoint.slideshow.macroEnabled.12",
                "application/vnd.openxmlformats-officedocument.presentationml.slideshow",
                // OpenDocument
                "application/vnd.oasis.opendocument.text",
                "application/vnd.oasis.opendocument.spreadsheet",
                "application/vnd.oasis.opendocument.presentation",
                // Rich Text
                "application/rtf",
                "text/rtf",
                // Text
                "text/plain",
                "text/markdown"
            }
        }
    };

    public static FileCategoryConfig GetConfiguration(FileCategory category)
    {
        if (_configurations.TryGetValue(category, out var config))
        {
            return config;
        }
        
        throw new ArgumentException($"Configuration not found for category: {category}");
    }

    public static bool IsValidCategory(FileCategory category)
    {
        return _configurations.ContainsKey(category);
    }

    public static string GetFolderPath(FileCategory category)
    {
        return GetConfiguration(category).FolderPath;
    }
}
