using AssignmentService.Domain.Enums;

namespace AssignmentService.EF.Entities;

/// <summary>
/// Entity đại diện cho bảng Problems
/// Đã gộp các thuộc tính từ ProblemVersion vào
/// </summary>
public class Problem
{
    public Guid ProblemId { get; set; }
    
    /// <summary>
    /// Mã đề bài (unique): P001, P002,...
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// URL-friendly slug (unique): two-sum, reverse-string,...
    /// </summary>
    public string Slug { get; set; } = string.Empty;
    
    /// <summary>
    /// Tiêu đề đề bài
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Độ khó: EASY, MEDIUM, HARD
    /// </summary>
    public Difficulty Difficulty { get; set; }
    
    /// <summary>
    /// ID người tạo đề bài
    /// </summary>
    public Guid OwnerId { get; set; }
    
    /// <summary>
    /// Quyền truy cập: PUBLIC, PRIVATE
    /// </summary>
    public Visibility Visibility { get; set; }
    
    /// <summary>
    /// Trạng thái: DRAFT, PUBLISHED, ARCHIVED
    /// </summary>
    public ProblemStatus Status { get; set; }
    
    // === Các thuộc tính từ ProblemVersion ===
    
    /// <summary>
    /// Đường dẫn file Markdown chứa đề bài
    /// </summary>
    public string? StatementMdRef { get; set; }
    
    /// <summary>
    /// Chế độ I/O: STDIO hoặc FILE
    /// </summary>
    public IoMode IoMode { get; set; } = IoMode.STDIO;
    
    /// <summary>
    /// Giới hạn thời gian (milliseconds)
    /// </summary>
    public int TimeLimitMs { get; set; } = 1000;
    
    /// <summary>
    /// Giới hạn bộ nhớ (KB)
    /// </summary>
    public int MemoryLimitKb { get; set; } = 262144; // 256 MB
    
    /// <summary>
    /// Giới hạn kích thước code (KB)
    /// </summary>
    public int SourceLimitKb { get; set; } = 65536; // 64 KB
    
    /// <summary>
    /// Giới hạn stack (KB)
    /// </summary>
    public int StackLimitKb { get; set; } = 8192; // 8 MB
    
    /// <summary>
    /// Đường dẫn script validator (custom checker)
    /// </summary>
    public string? ValidatorRef { get; set; }
    
    /// <summary>
    /// Ghi chú thay đổi
    /// </summary>
    public string? Changelog { get; set; }
    
    /// <summary>
    /// Có bị khóa không (không cho sửa)
    /// </summary>
    public bool IsLocked { get; set; } = false;
    
    // === Additional Fields for Desktop App ===
    
    /// <summary>
    /// Problem description/statement (for students)
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Sample input for the problem
    /// </summary>
    public string? SampleInput { get; set; }
    
    /// <summary>
    /// Sample output for the problem
    /// </summary>
    public string? SampleOutput { get; set; }
    
    /// <summary>
    /// Problem constraints
    /// </summary>
    public string? Constraints { get; set; }
    
    // === Timestamps ===
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

    // === Navigation Properties ===
    // public User Owner { get; set; } = null!;
    
    /// <summary>
    /// Collection các tags gắn với đề bài
    /// </summary>
    public ICollection<ProblemTag> ProblemTags { get; set; } = new List<ProblemTag>();
    
    /// <summary>
    /// Collection các datasets (test cases)
    /// </summary>
    public ICollection<Dataset> Datasets { get; set; } = new List<Dataset>();
    
    /// <summary>
    /// Collection các code templates
    /// </summary>
    public ICollection<CodeTemplate> CodeTemplates { get; set; } = new List<CodeTemplate>();
    
    /// <summary>
    /// Collection các assets (PDF, images,...)
    /// </summary>
    public ICollection<ProblemAsset> ProblemAssets { get; set; } = new List<ProblemAsset>();
    
    /// <summary>
    /// Collection các language limits
    /// </summary>
    public ICollection<LanguageLimit> LanguageLimits { get; set; } = new List<LanguageLimit>();

}