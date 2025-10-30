using ProblemService.Domain.Enums;

namespace ProblemService.Domain.Entities;

/// <summary>
/// Entity đại diện cho bảng ProblemAssets
/// Tài nguyên đính kèm đề bài (PDF, images, attachments)
/// </summary>
public class ProblemAsset
{
    public Guid ProblemAssetId { get; set; }
    
    /// <summary>
    /// Foreign Key đến Problem (thay vì ProblemVersion)
    /// </summary>
    public Guid ProblemId { get; set; }
    
    /// <summary>
    /// Loại tài nguyên: STATEMENT_PDF, IMAGE, ATTACHMENT
    /// </summary>
    public AssetType Type { get; set; }
    
    /// <summary>
    /// Đường dẫn file trên object storage (S3, Azure Blob,...)
    /// </summary>
    public string ObjectRef { get; set; } = string.Empty;
    
    /// <summary>
    /// SHA-256 checksum để verify tính toàn vẹn file
    /// </summary>
    public string? Checksum { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /* ===== NAVIGATION PROPERTIES ===== */
    
    public Problem Problem { get; set; } = null!;
}