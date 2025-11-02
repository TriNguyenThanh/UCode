namespace AssignmentService.Domain.Enums;

/// <summary>
/// Định dạng nội dung asset
/// </summary>
public enum ContentFormat
{
    /// <summary>
    /// Nội dung Markdown
    /// </summary>
    MARKDOWN,
    
    /// <summary>
    /// Nội dung HTML
    /// </summary>
    HTML,
    
    /// <summary>
    /// URL/Link tới tài nguyên bên ngoài
    /// </summary>
    URL,
    
    /// <summary>
    /// Plain text
    /// </summary>
    TEXT,
    
    /// <summary>
    /// JSON data
    /// </summary>
    JSON,
    
    /// <summary>
    /// File binary (image, pdf, video)
    /// </summary>
    BINARY
}