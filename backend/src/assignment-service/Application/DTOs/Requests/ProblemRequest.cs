using AssignmentService.Domain.Enums;
using AssignmentService.Application.DTOs.Common;
using System.ComponentModel.DataAnnotations;

namespace AssignmentService.Application.DTOs.Requests;

/// <summary>
/// DTO cho request tạo/cập nhật Problem
/// </summary>
public class ProblemRequest
{ 

  public Guid ProblemId { get; set; } = Guid.Empty;

    /// <summary>
  /// Mã đề bài (unique): P001, P002,...
  /// </summary>
  [Required(ErrorMessage = "Code is required")]
    [StringLength(50, ErrorMessage = "Code must not exceed 50 characters")]
    public string Code { get; set; } = string.Empty;
    
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
    public string? StatementMdRef { get; set; }
    
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
    /// Danh sách Language Limits (bao gồm code templates)
    /// </summary>
    public List<LanguageLimitDto>? LanguageLimits { get; set; }
    
    /// <summary>
    /// Danh sách Assets (PDF, images,...) - For CREATE only
    /// </summary>
    public List<CreateProblemAssetDto>? ProblemAssets { get; set; }
}

/*

{ DTOs ===
  "code": "P001",
  "slug": "two-sum",ublic class DatasetDto
  "title": "Two Sum",
  "difficulty": "EASY",
  "ownerId": "22222222-2222-2222-2222-222222222222",
  "visibility": "PUBLIC",public string Name { get; set; } = string.Empty;
  "status": "PUBLISHED",
  "statementMdRef": "problems/two-sum/statement.md",public DatasetKind Kind { get; set; } = DatasetKind.SAMPLE;
  "ioMode": "STDIO",
  "timeLimitMs": 1000,   public List<TestCaseDto>? TestCases { get; set; }
  "memoryLimitKb": 262144,}
  "sourceLimitKb": 65536,
  "stackLimitKb": 8192,ublic class TestCaseDto
  "validatorRef": null,
  "changelog": "Initial version - Classic two sum problem",
  "isLocked": false,
  "tagIds": [public string InputRef { get; set; } = string.Empty;
    "11111111-1111-1111-1111-111111111111",
    "22222222-2222-2222-2222-222222222222"
  ],
  "datasets": [public string OutputRef { get; set; } = string.Empty;
    {
      "name": "Sample",
      "kind": "SAMPLE",public decimal Weight { get; set; } = 1.0m;
      "testCases": [
        {
          "inputRef": "testcases/two-sum/sample/input1.txt",public string? InputChecksum { get; set; }
          "outputRef": "testcases/two-sum/sample/output1.txt",
          "weight": 1.0,
          "inputChecksum": "abc123",   public string? OutputChecksum { get; set; }
          "outputChecksum": "def456"}
        },
        {ublic class LanguageLimitDto
          "inputRef": "testcases/two-sum/sample/input2.txt",
          "outputRef": "testcases/two-sum/sample/output2.txt",
          "weight": 1.0,
          "inputChecksum": "ghi789",public string Lang { get; set; } = string.Empty;
          "outputChecksum": "jkl012"
        }
      ]public decimal TimeFactor { get; set; } = 1.0m;
    },
    {
      "name": "Test",   public int? MemoryKbOverride { get; set; }
      "kind": "TEST",}
      "testCases": [
        {ublic class CodeTemplateDto
          "inputRef": "testcases/two-sum/test/input1.txt",
          "outputRef": "testcases/two-sum/test/output1.txt",
          "weight": 2.0,
          "inputChecksum": "test123",public string Lang { get; set; } = string.Empty;
          "outputChecksum": "test456"
        }
      ]
    }   public string StarterRef { get; set; } = string.Empty;
  ],}
  "languageLimits": [
    {ublic class ProblemAssetDto
      "lang": "python",
      "timeFactor": 3.0,
      "memoryKbOverride": 524288    public AssetType Type { get; set; }
    },
    {
      "lang": "java",
      "timeFactor": 2.0,    public string ObjectRef { get; set; } = string.Empty;
      "memoryKbOverride": null
    }
  ],   public string? Checksum { get; set; }
  "codeTemplates": [}
    {
      "lang": "cpp",/*
      "starterRef": "templates/two-sum/cpp/starter.cpp"
    },
    {
      "lang": "python",
      "starterRef": "templates/two-sum/python/starter.py"
    },
    {22-2222-2222-222222222222",
      "lang": "java",,
      "starterRef": "templates/two-sum/java/Main.java"
    }"problems/two-sum/statement.md",
  ],
  "problemAssets": [
    {,
      "type": "PDF",6,
      "objectRef": "assets/two-sum/explanation.pdf",
      "checksum": "pdf123abc"
    },ial version - Classic two sum problem",
    { false,
      "type": "IMAGE",
      "objectRef": "assets/two-sum/diagram.png",,
      "checksum": "img456def""22222222-2222-2222-2222-222222222222"
    }
  ]tasets": [
}

*/
/* Simple ExamplestCases": [
{
  "code": "P002",
  "slug": "reverse-string",testcases/two-sum/sample/output1.txt",
  "title": "Reverse String",
  "difficulty": "EASY",
  "ownerId": "22222222-2222-2222-2222-222222222222","outputChecksum": "def456"
  "visibility": "PRIVATE",,
  "status": "DRAFT",
  "ioMode": "STDIO",
  "timeLimitMs": 1000,testcases/two-sum/sample/output2.txt",
  "memoryLimitKb": 262144,
  "sourceLimitKb": 65536,
  "stackLimitKb": 8192, "outputChecksum": "jkl012"
  "isLocked": false, }
  "tagIds": []]
},
*/