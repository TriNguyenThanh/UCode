namespace AssignmentService.EF.Entities;

/// <summary>
/// Entity đại diện cho bảng TestCases
/// Mỗi TestCase là 1 bộ input/output để test code
/// </summary>
public class TestCase
{
    public Guid TestCaseId { get; set; }
    
    /// <summary>
    /// Foreign Key đến Dataset
    /// </summary>
    public Guid DatasetId { get; set; }
    
    /// <summary>
    /// Thứ tự test case trong dataset (0, 1, 2,...)
    /// </summary>
    public int IndexNo { get; set; }
    
    /// <summary>
    /// Đường dẫn file chứa input data
    /// Lưu trên file storage
    /// </summary>
    public string InputRef { get; set; } = string.Empty;
    
    /// <summary>
    /// Đường dẫn file chứa expected output
    /// </summary>
    public string OutputRef { get; set; } = string.Empty;
    
    /// <summary>
    /// Trọng số điểm của test này
    /// VD: 1.0 = điểm chuẩn, 2.0 = gấp đôi điểm
    /// </summary>
    public decimal Weight { get; set; } = 1.00m;
    
    /// <summary>
    /// Đánh dấu test mẫu (hiển thị trong đề bài)
    /// </summary>
    public bool IsSample { get; set; }
    
    /* ===== NAVIGATION PROPERTIES ===== */
    
    /// <summary>
    /// Reference về Dataset chứa test này
    /// </summary>
    public Dataset Dataset { get; set; } = null!;
}