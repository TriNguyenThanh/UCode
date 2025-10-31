namespace AssignmentService.Domain.Entities;

/// <summary>
/// Entity đại diện cho bảng TestCaseFiles
/// Mỗi TestCaseFile là 1 bộ input/output để test code
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
    /// Điểm cho test case này (score)
    /// VD: "100" = 100 điểm
    /// </summary>
    public string Score { get; set; } = "100";
    
    /* ===== NAVIGATION PROPERTIES ===== */
    
    /// <summary>
    /// Reference về Dataset chứa test này
    /// </summary>
    public Dataset Dataset { get; set; } = null!;
}