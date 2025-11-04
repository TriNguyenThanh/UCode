using file_service.Enums;

namespace file_service.Validators;

public static class FileValidator
{
    /// <summary>
    /// Validate file theo category
    /// </summary>
    public static (bool IsValid, string? ErrorMessage) ValidateFile(
        IFormFile file, 
        FileCategory category)
    {
        if (file == null || file.Length == 0)
        {
            return (false, "File is empty or null");
        }

        var config = Models.FileCategoryConfiguration.GetConfiguration(category);

        // 1. Kiểm tra file size
        if (file.Length > config.MaxFileSizeBytes)
        {
            var maxSizeMB = config.MaxFileSizeBytes / (1024.0 * 1024.0);
            return (false, $"File size exceeds maximum limit of {maxSizeMB:F2}MB for {category}");
        }

        // 2. Kiểm tra file extension
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(fileExtension))
        {
            return (false, "File must have an extension");
        }

        if (!config.AllowedExtensions.Contains(fileExtension))
        {
            return (false, $"File extension '{fileExtension}' is not allowed for {category}. " +
                          $"Allowed extensions: {string.Join(", ", config.AllowedExtensions)}");
        }

        // 3. Kiểm tra MIME type
        if (!string.IsNullOrEmpty(file.ContentType))
        {
            var isValidMimeType = config.AllowedMimeTypes.Any(mime => 
                file.ContentType.StartsWith(mime, StringComparison.OrdinalIgnoreCase));
            
            if (!isValidMimeType)
            {
                return (false, $"File type '{file.ContentType}' is not allowed for {category}");
            }
        }

        // 4. Kiểm tra tên file (security)
        if (ContainsDangerousCharacters(file.FileName))
        {
            return (false, "File name contains invalid or dangerous characters");
        }

        return (true, null);
    }

    /// <summary>
    /// Kiểm tra tên file có chứa ký tự nguy hiểm
    /// </summary>
    private static bool ContainsDangerousCharacters(string fileName)
    {
        // Các ký tự nguy hiểm trong tên file
        var dangerousChars = new[] { "..", "/", "\\", ":", "*", "?", "\"", "<", ">", "|", "\0" };
        
        return dangerousChars.Any(fileName.Contains);
    }

    /// <summary>
    /// Sanitize file name để đảm bảo an toàn
    /// </summary>
    public static string SanitizeFileName(string fileName)
    {
        // Loại bỏ path và chỉ giữ lại tên file
        fileName = Path.GetFileName(fileName);
        
        // Loại bỏ các ký tự đặc biệt
        var invalidChars = Path.GetInvalidFileNameChars();
        foreach (var c in invalidChars)
        {
            fileName = fileName.Replace(c, '_');
        }

        // Loại bỏ khoảng trắng thừa
        fileName = fileName.Trim();

        // Giới hạn độ dài tên file
        var extension = Path.GetExtension(fileName);
        var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
        
        if (nameWithoutExt.Length > 100)
        {
            nameWithoutExt = nameWithoutExt.Substring(0, 100);
        }

        return nameWithoutExt + extension;
    }

    /// <summary>
    /// Validate file content (check magic bytes)
    /// </summary>
    public static async Task<bool> ValidateFileContentAsync(IFormFile file, FileCategory category)
    {
        var config = Models.FileCategoryConfiguration.GetConfiguration(category);
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        // Chỉ validate cho một số loại file quan trọng
        if (category == FileCategory.Image || category == FileCategory.Avatar)
        {
            return await ValidateImageMagicBytesAsync(file);
        }

        return true;
    }

    /// <summary>
    /// Validate image bằng magic bytes (file signature)
    /// </summary>
    private static async Task<bool> ValidateImageMagicBytesAsync(IFormFile file)
    {
        try
        {
            using var stream = file.OpenReadStream();
            var buffer = new byte[8];
            await stream.ReadAsync(buffer, 0, buffer.Length);

            // JPEG: FF D8 FF
            if (buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF)
                return true;

            // PNG: 89 50 4E 47 0D 0A 1A 0A
            if (buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47)
                return true;

            // GIF: 47 49 46 38
            if (buffer[0] == 0x47 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x38)
                return true;

            // WEBP: 52 49 46 46 ... 57 45 42 50
            if (buffer[0] == 0x52 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x46)
            {
                // Check WEBP signature at bytes 8-11
                stream.Seek(8, SeekOrigin.Begin);
                var webpBuffer = new byte[4];
                await stream.ReadAsync(webpBuffer, 0, 4);
                if (webpBuffer[0] == 0x57 && webpBuffer[1] == 0x45 && 
                    webpBuffer[2] == 0x42 && webpBuffer[3] == 0x50)
                    return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}
