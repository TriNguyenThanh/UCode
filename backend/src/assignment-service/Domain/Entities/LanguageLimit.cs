namespace AssignmentService.Domain.Entities;

/// <summary>
/// Entity đại diện cho bảng LanguageLimits
/// Giới hạn riêng theo từng ngôn ngữ lập trình + Code Template
/// </summary>
public class LanguageLimit
{
    public Guid LanguageLimitId { get; set; }
    
    /// <summary>
    /// Foreign Key đến Problem (thay vì ProblemVersion)
    /// </summary>
    public Guid ProblemId { get; set; }
    
    /// <summary>
    /// Mã ngôn ngữ (VD: "cpp17", "java17", "python3")
    /// </summary>
    public string Lang { get; set; } = string.Empty;
    
    /// <summary>
    /// Hệ số nhân thời gian cho ngôn ngữ này
    /// VD: Python = 2.5 (vì Python chạy chậm hơn C++)
    /// Nullable vì nếu NULL sẽ dùng giá trị mặc định
    /// </summary>
    public decimal? TimeFactor { get; set; }
    
    /// <summary>
    /// Ghi đè memory limit cho ngôn ngữ này (KB)
    /// Nullable = nếu NULL thì dùng memory limit chung
    /// </summary>
    public int? MemoryKbOverride { get; set; }
    
    /// <summary>
    /// Code template - phần đầu (imports, setup)
    /// </summary>
    public string? Head { get; set; }
    
    /// <summary>
    /// Code template - phần chính (user code placeholder)
    /// </summary>
    public string? Body { get; set; }
    
    /// <summary>
    /// Code template - phần cuối (test runner, main function)
    /// </summary>
    public string? Tail { get; set; }

    // Navigation Properties
    public Problem Problem { get; set; } = null!;
}