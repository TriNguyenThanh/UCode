namespace AssignmentService.Domain.Entities;

/// <summary>
/// Problem-specific language configuration overrides
/// CHỈ lưu khi teacher/admin cần CUSTOMIZE, không lưu default values
/// Cho phép override global Language config cho từng problem cụ thể
/// Composite PK: (ProblemId, LanguageId)
/// </summary>
public class ProblemLanguage
{
    // Composite Primary Key - configured in ProblemLanguageConfiguration
    public Guid ProblemId { get; set; }
    public Guid LanguageId { get; set; }
    
    /// <summary>
    /// Override time factor (NULL = dùng Language.DefaultTimeFactor)
    /// Example: Problem đặc biệt cần Python chạy nhanh hơn → set 2.0 thay vì 2.5
    /// </summary>
    public decimal? TimeFactorOverride { get; set; }
    
    /// <summary>
    /// Override memory limit (NULL = dùng Language.DefaultMemoryKb hoặc Problem.MemoryLimitKb)
    /// Example: Problem cần nhiều memory cho Python → override higher
    /// </summary>
    public int? MemoryKbOverride { get; set; }
    
    /// <summary>
    /// Custom code template - Head (NULL = dùng Language.DefaultHead)
    /// </summary>
    public string? HeadOverride { get; set; }
    
    /// <summary>
    /// Custom code template - Body (NULL = dùng Language.DefaultBody)
    /// </summary>
    public string? BodyOverride { get; set; }
    
    /// <summary>
    /// Custom code template - Tail (NULL = dùng Language.DefaultTail)
    /// </summary>
    public string? TailOverride { get; set; }
    
    /// <summary>
    /// Có allow ngôn ngữ này cho problem này không
    /// Teacher có thể disable Python cho bài yêu cầu implement bằng C++
    /// </summary>
    public bool IsAllowed { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Problem Problem { get; set; } = null!;
    public Language Language { get; set; } = null!;
}
