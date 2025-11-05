namespace AssignmentService.Domain.Entities;

/// <summary>
/// Global language configuration (system-wide defaults)
/// Chứa cấu hình mặc định cho TẤT CẢ problems
/// </summary>
public class Language
{
    public Guid LanguageId { get; set; }
    
    /// <summary>
    /// Mã ngôn ngữ (cpp, java, python, javascript, csharp, rust, go...)
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Tên hiển thị (C++17, Java 17, Python 3.11, Node.js 18...)
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// Default time factor - Hệ số nhân cho time limit
    /// Python chậm hơn C++ nên cần time factor cao hơn (Python=2.5, C++=1.0)
    /// </summary>
    public decimal DefaultTimeFactor { get; set; } = 1.0m;
    
    /// <summary>
    /// Default memory limit override (KB)
    /// NULL = use problem's default memory limit
    /// </summary>
    public int? DefaultMemoryKb { get; set; }
    
    /// <summary>
    /// Default code template - Head (imports, initial setup)
    /// </summary>
    public string? DefaultHead { get; set; }
    
    /// <summary>
    /// Default code template - Body (main function, user code placeholder)
    /// </summary>
    public string? DefaultBody { get; set; }
    
    /// <summary>
    /// Default code template - Tail (test runner, output handler)
    /// </summary>
    public string? DefaultTail { get; set; }
    
    /// <summary>
    /// Có được enable không - Admin có thể disable ngôn ngữ
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// Thứ tự hiển thị trong UI (1 = hiển thị đầu tiên)
    /// </summary>
    public int DisplayOrder { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<ProblemLanguage> ProblemLanguages { get; set; } = new List<ProblemLanguage>();
}
