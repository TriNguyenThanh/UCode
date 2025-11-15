using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AssignmentService.Domain.Enums;

namespace AssignmentService.Domain.Entities;

public class Submission
{
    public Guid SubmissionId { get; set; }
    
    public Guid UserId { get; set; }
    
    public Guid? AssignmentId { get; set; }
    
    public Guid ProblemId { get; set; }
    
    /// <summary>
    /// Foreign Key đến Dataset được sử dụng để chấm
    /// </summary>
    public Guid DatasetId { get; set; }

    /// <summary>
    /// User name của người nộp bài
    /// </summary>
    public string UserFullName { get; set; } = string.Empty;

    /// <summary>
    /// User code của người nộp bài
    /// </summary>
    public string UserCode { get; set; } = string.Empty;

    /// <summary>
    /// Source code của bài nộp
    /// </summary>
    public string SourceCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Đường dẫn hoặc reference đến source code (lưu trên storage)
    /// </summary>
    public string SourceCodeRef { get; set; } = string.Empty;

    /// <summary>
    /// Ngôn ngữ lập trình
    /// </summary>
    public Guid LanguageId { get; set; }

        /// <summary>
    /// Ngôn ngữ lập trình
    /// </summary>
    public string LanguageCode { get; set; } = string.Empty;

    /// <summary>
    /// Kết quả so sánh (compare result)
    /// </summary>
    public string? CompareResult { get; set; }

    /// <summary>
    /// Trạng thái submission
    /// </summary>
    public SubmissionStatus Status { get; set; } = SubmissionStatus.Pending;

    /// <summary>
    /// Trạng thái submission
    /// </summary>
    public bool isSubmitLate { get; set; } = false;
    
    /// <summary>
    /// Mã lỗi (nếu có)
    /// </summary>
    public string? ErrorCode { get; set; }
    
    /// <summary>
    /// Thông báo lỗi chi tiết
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Tổng số test case
    /// </summary>
    public int TotalTestcase { get; set; } = 0;

    /// <summary>
    /// Số test case passed
    /// </summary>
    public int PassedTestcase { get; set; } = 0;

    /// <summary>
    /// Số test case passed
    /// </summary>
    public int Score { get; set; } = 0;
    
    /// <summary>
    /// Nhận xét của submission
    /// </summary>
    public string Comment { get; set; } = string.Empty;

    /// <summary>
    /// Tổng thời gian thực thi (ms)
    /// </summary>
    public long TotalTime { get; set; } = 0;
    
    /// <summary>
    /// Tổng bộ nhớ sử dụng (KB)
    /// </summary>
    public long TotalMemory { get; set; } = 0;
    
    /// <summary>
    /// Thời gian nộp bài
    /// </summary>
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Tham chiếu đến file kết quả (lưu trên storage)
    /// </summary>
    public string? ResultFileRef { get; set; }

    // Navigation Properties
    public Assignment Assignment { get; set; } = null!;
    public Problem Problem { get; set; } = null!;
    public Dataset Dataset { get; set; } = null!;
    public Language Language { get; set; } = null!;
}
