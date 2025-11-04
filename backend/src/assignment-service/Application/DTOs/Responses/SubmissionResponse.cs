using AssignmentService.Domain.Enums;

namespace AssignmentService.Application.DTOs.Responses;

public class SubmissionResponse
{
    /// <summary>
    /// Đường dẫn hoặc reference đến source code (lưu trên storage)
    /// </summary>
    public string SourceCodeRef { get; set; } = string.Empty;
    
    /// <summary>
    /// Ngôn ngữ lập trình
    /// </summary>
    public string Language { get; set; } = string.Empty;
    
    /// <summary>
    /// Trạng thái submission
    /// </summary>
    public SubmissionStatus Status { get; set; } = SubmissionStatus.Pending;
    
    /// <summary>
    /// Kết quả so sánh (compare result)
    /// </summary>
    public string? CompareResult { get; set; }
    
    /// <summary>
    /// Mã lỗi (nếu có)
    /// </summary>
    public string? ErrorCode { get; set; }
    
    /// <summary>
    /// Thông báo lỗi chi tiết
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Tổng thời gian thực thi (ms)
    /// </summary>
    public long TotalTime { get; set; } = 0;
    
    /// <summary>
    /// Tổng bộ nhớ sử dụng (KB)
    /// </summary>
    public long TotalMemory { get; set; } = 0;
    
    /// <summary>
    /// Tham chiếu đến file kết quả (lưu trên storage)
    /// </summary>
    public string? ResultFileRef { get; set; }
    
    /// <summary>
    /// Thời gian nộp bài
    /// </summary>
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}

