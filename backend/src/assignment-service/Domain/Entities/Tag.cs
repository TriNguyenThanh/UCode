using AssignmentService.Domain.Enums;

namespace AssignmentService.Domain.Entities;

/// <summary>
/// Entity đại diện cho bảng Tags
/// Tag để phân loại đề bài (Array, String, DP,...)
/// </summary>
public class Tag
{
    public Guid TagId { get; set; }
    
    /// <summary>
    /// Tên tag (VD: "Array", "Dynamic Programming")
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Phân loại tag: TOPIC, DIFFICULTY, OTHER
    /// </summary>
    public TagCategory Category { get; set; }
    
    /* ===== NAVIGATION PROPERTIES ===== */
    
    /// <summary>
    /// Many-to-Many relationship với Problems
    /// Thông qua bảng trung gian ProblemTags
    /// </summary>
    public ICollection<ProblemTag> ProblemTags { get; set; } = new List<ProblemTag>();
}