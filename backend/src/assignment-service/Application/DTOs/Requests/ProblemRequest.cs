using AssignmentService.Domain.Enums;
using AssignmentService.Application.DTOs.Common;
using System.ComponentModel.DataAnnotations;

namespace AssignmentService.Application.DTOs.Requests
{
  /// <summary>
  /// DTO cho request tạo/cập nhật Problem
  /// </summary>
  public class ProblemRequest
  {

    public Guid ProblemId { get; set; } = Guid.Empty;

    /// <summary>
    /// Mã đề bài (unique): P001, P002,...
    /// </summary>
    [StringLength(50, ErrorMessage = "Code must not exceed 50 characters")]
    public string? Code { get; set; } = string.Empty;

    /// <summary>
    /// URL-friendly slug (unique): two-sum, reverse-string,...
    /// </summary>
    // [Required(ErrorMessage = "Slug is required")]
    // [StringLength(200, ErrorMessage = "Slug must not exceed 200 characters")]
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Tiêu đề đề bài
    /// </summary>
    [Required(ErrorMessage = "Title is required")]
    [StringLength(500, ErrorMessage = "Title must not exceed 500 characters")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Độ khó: EASY, MEDIUM, HARD
    /// </summary>
    [Required(ErrorMessage = "Difficulty is required")]
    public Difficulty Difficulty { get; set; } = Difficulty.EASY;

    /// <summary>
    /// ID người tạo đề bài
    /// </summary>
    public Guid? OwnerId { get; set; }

    /// <summary>
    /// Quyền truy cập: PUBLIC, PRIVATE
    /// </summary>
    public Visibility Visibility { get; set; } = Visibility.PRIVATE;

    /// <summary>
    /// Trạng thái: DRAFT, PUBLISHED, ARCHIVED
    /// </summary>
    public ProblemStatus Status { get; set; } = ProblemStatus.DRAFT;

    /// <summary>
    /// Đường dẫn file Markdown chứa đề bài
    /// </summary>
    [StringLength(500)]
    public string? Statement { get; set; }

    /// <summary>
    /// Giải pháp tham khảo (reference solution)
    /// </summary>
    [StringLength(2000)]
    public string? Solution { get; set; }

    /// <summary>
    /// Chế độ I/O: STDIO hoặc FILE
    /// </summary>
    public IoMode IoMode { get; set; } = IoMode.STDIO;

    /// <summary>
    /// Định dạng input (mô tả cách đọc input)
    /// </summary>
    [StringLength(1000)]
    public string? InputFormat { get; set; }

    /// <summary>
    /// Định dạng output (mô tả cách xuất output)
    /// </summary>
    [StringLength(1000)]
    public string? OutputFormat { get; set; }

    /// <summary>
    /// Ràng buộc của bài toán
    /// </summary>
    [StringLength(2000)]
    public string? Constraints { get; set; }

    /// <summary>
    /// Giới hạn thời gian (milliseconds)
    /// </summary>
    [Range(100, 30000, ErrorMessage = "Time limit must be between 100ms and 30000ms")]
    public int TimeLimitMs { get; set; } = 1000;

    /// <summary>
    /// Giới hạn bộ nhớ (KB)
    /// </summary>
    [Range(1024, 1048576, ErrorMessage = "Memory limit must be between 1MB and 1024MB")]
    public int MemoryLimitKb { get; set; } = 262144; // 256 MB

    /// <summary>
    /// Giới hạn kích thước code (KB)
    /// </summary>
    [Range(1, 102400, ErrorMessage = "Source limit must be between 1KB and 100MB")]
    public int SourceLimitKb { get; set; } = 65536; // 64 KB

    /// <summary>
    /// Giới hạn stack (KB)
    /// </summary>
    [Range(1024, 32768, ErrorMessage = "Stack limit must be between 1MB and 32MB")]
    public int StackLimitKb { get; set; } = 8192; // 8 MB

    /// <summary>
    /// Đường dẫn script validator (custom checker)
    /// </summary>
    [StringLength(500)]
    public string? ValidatorRef { get; set; }

    /// <summary>
    /// Ghi chú thay đổi
    /// </summary>
    [StringLength(2000)]
    public string? Changelog { get; set; }

    /// <summary>
    /// Có bị khóa không (không cho sửa)
    /// </summary>
    public bool IsLocked { get; set; } = false;

    // === Related Data - Sử dụng Common DTOs ===

    /// <summary>
    /// Danh sách Tag IDs gắn với đề bài
    /// </summary>
    public List<Guid> TagIds { get; set; } = new List<Guid>();

    /// <summary>
    /// Danh sách Datasets (test cases)
    /// </summary>
    public List<DatasetDto>? Datasets { get; set; }

    /// <summary>
    /// Danh sách Language configurations (bao gồm code templates và overrides)
    /// </summary>
    public List<ProblemLanguageDto>? LanguageLimits { get; set; }

    /// <summary>
    /// Danh sách Assets (PDF, images,...) - For CREATE only
    /// </summary>
    public List<CreateProblemAssetDto>? ProblemAssets { get; set; }
  }


  public class ProblemCreateRequest
  {
    /// <summary>
    /// Unique problem code (e.g., P001, P002)
    /// </summary>
    /// <example>P001</example>
    [StringLength(50, ErrorMessage = "Code must not exceed 50 characters")]
    public string? Code { get; set; } = string.Empty;

    /// <summary>
    /// Problem title
    /// </summary>
    /// <example>Two Sum</example>
    [Required(ErrorMessage = "Title is required")]
    [StringLength(500, ErrorMessage = "Title must not exceed 500 characters")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Problem difficulty level
    /// </summary>
    /// <example>EASY</example>
    [Required(ErrorMessage = "Difficulty is required")]
    public Difficulty Difficulty { get; set; } = Difficulty.EASY;

    /// <summary>
    /// Problem visibility setting
    /// </summary>
    /// <example>PUBLIC</example>
    public Visibility Visibility { get; set; } = Visibility.PRIVATE;

    /// <summary>
    /// Problem status
    /// </summary>
    /// <example>DRAFT</example>
    public ProblemStatus Status { get; set; } = ProblemStatus.DRAFT;

    /// <summary>
    /// Time limit in milliseconds
    /// </summary>
    /// <example>1000</example>
    [Range(100, 30000, ErrorMessage = "Time limit must be between 100ms and 30000ms")]
    public int TimeLimitMs { get; set; } = 1000;

    /// <summary>
    /// Memory limit in KB
    /// </summary>
    /// <example>262144</example>
    [Range(1024, 1048576, ErrorMessage = "Memory limit must be between 1MB and 1024MB")]
    public int MemoryLimitKb { get; set; } = 262144;

    /// <summary>
    /// Source code size limit in KB
    /// </summary>
    /// <example>65536</example>
    [Range(1, 102400, ErrorMessage = "Source limit must be between 1KB and 100MB")]
    public int SourceLimitKb { get; set; } = 65536;

    /// <summary>
    /// Stack size limit in KB
    /// </summary>
    /// <example>8192</example>
    [Range(1024, 32768, ErrorMessage = "Stack limit must be between 1MB and 32MB")]
    public int StackLimitKb { get; set; } = 8192;

    /// <summary>
    /// I/O mode for the problem
    /// </summary>
    /// <example>STDIO</example>
    public IoMode IoMode { get; set; } = IoMode.STDIO;

    /// <summary>
    /// Reference to problem statement markdown file
    /// </summary>
    /// <example>problems/two-sum/statement.md</example>
    [StringLength(500)]
    public string? Statement { get; set; }

    /// <summary>
    /// Reference to custom validator script
    /// </summary>
    /// <example>validators/two-sum/validator.py</example>
    [StringLength(500)]
    public string? ValidatorRef { get; set; }

    /// <summary>
    /// Changelog notes for this version
    /// </summary>
    /// <example>Initial version - Classic two sum problem</example>
    [StringLength(2000)]
    public string? Changelog { get; set; }

    /// <summary>
    /// Whether the problem is locked from editing
    /// </summary>
    /// <example>false</example>
    public bool IsLocked { get; set; } = false;

    /// <summary>
    /// List of tag IDs associated with this problem
    /// </summary>
    /// <example>["11111111-1111-1111-1111-111111111111", "22222222-2222-2222-2222-222222222222"]</example>
    public List<Guid> TagIds { get; set; } = new List<Guid>();
  }
}