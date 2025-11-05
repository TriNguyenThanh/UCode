using AssignmentService.Domain.Enums;

namespace AssignmentService.Domain.Entities;

/// <summary>
/// Entity đại diện cho bảng ProblemAssets
/// Lưu trữ các tài nguyên liên quan đến Problem (images, videos, solutions, tutorials,...)
/// </summary>
public class ProblemAsset
{
    public Guid ProblemAssetId { get; set; }
    
    /// <summary>
    /// ID của Problem
    /// </summary>
    public Guid ProblemId { get; set; }
    
    /// <summary>
    /// Loại asset: IMAGE, VIDEO, PDF, STATEMENT, SOLUTION, TUTORIAL, HINT
    /// </summary>
    public AssetType Type { get; set; }
    
    /// <summary>
    /// Reference tới object (S3/Blob URL hoặc inline content)
    /// </summary>
    public string ObjectRef { get; set; } = string.Empty;
    
    /// <summary>
    /// Checksum để verify integrity
    /// </summary>
    public string? Checksum { get; set; }
    
    /// <summary>
    /// Tiêu đề hiển thị (optional)
    /// </summary>
    public string? Title { get; set; }
    
    /// <summary>
    /// Định dạng nội dung: MARKDOWN, HTML, URL, TEXT, JSON, BINARY
    /// </summary>
    public ContentFormat Format { get; set; }
    
    /// <summary>
    /// Thứ tự hiển thị
    /// </summary>
    public int OrderIndex { get; set; }
    
    /// <summary>
    /// Asset có đang active không
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // === Navigation Properties ===
    public Problem Problem { get; set; } = null!;
}