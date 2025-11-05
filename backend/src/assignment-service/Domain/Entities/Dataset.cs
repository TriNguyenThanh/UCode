using AssignmentService.Domain.Enums;

namespace AssignmentService.Domain.Entities;

/// <summary>
/// Entity đại diện cho bảng Datasets
/// Dataset là nhóm các test cases (SAMPLE, PUBLIC, PRIVATE, OFFICIAL)
/// </summary>
public class Dataset
{
    public Guid DatasetId { get; set; }
    
    /// <summary>
    /// Foreign Key đến Problem (thay vì ProblemVersion)
    /// </summary>
    public Guid ProblemId { get; set; }
    
    /// <summary>
    /// Tên dataset (VD: "Sample Tests", "Basic", "Advanced")
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Loại dataset: SAMPLE/PUBLIC/PRIVATE/OFFICIAL
    /// </summary>
    public DatasetKind Kind { get; set; }

    /* ===== NAVIGATION PROPERTIES ===== */
    
    /// <summary>
    /// Reference về Problem chứa dataset này
    /// </summary>
    public Problem Problem { get; set; } = null!;
    
    /// <summary>
    /// Collection các TestCases thuộc dataset này
    /// 1 Dataset có nhiều TestCases (One-to-Many)
    /// </summary>
    public ICollection<TestCase> TestCases { get; set; } = new List<TestCase>();
}