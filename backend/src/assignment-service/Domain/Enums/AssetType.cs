namespace AssignmentService.Domain.Enums;

/// <summary>
/// Loại asset của Problem
/// </summary>
public enum AssetType
{
    /// <summary>
    /// Hình ảnh minh họa
    /// </summary>
    IMAGE,
    
    /// <summary>
    /// Video hướng dẫn
    /// </summary>
    VIDEO,
    
    /// <summary>
    /// File PDF
    /// </summary>
    PDF,
    
    /// <summary>
    /// Đề bài chi tiết
    /// </summary>
    STATEMENT,
    
    /// <summary>
    /// Lời giải chi tiết
    /// </summary>
    SOLUTION,
    
    /// <summary>
    /// Hướng dẫn giải bài
    /// </summary>
    TUTORIAL,
    
    /// <summary>
    /// Gợi ý
    /// </summary>
    HINT,
    
    /// <summary>
    /// Editorial/Phân tích bài toán
    /// </summary>
    EDITORIAL,
    
    /// <summary>
    /// File đính kèm khác
    /// </summary>
    ATTACHMENT
}