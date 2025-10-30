namespace ProblemService.Domain.Entities;

/// <summary>
/// Entity đại diện cho bảng Users
/// Đây là stub table (tham chiếu từ UserService)
/// </summary>
public class User
{
    /// <summary>
    /// Khóa chính - Primary Key
    /// Guid = UNIQUEIDENTIFIER trong SQL Server
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Email người dùng
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /* ===== NAVIGATION PROPERTIES =====
     * Navigation Properties là các thuộc tính đại diện cho relationships
     * Không được lưu trực tiếp vào database, chỉ dùng để navigate giữa các entities
     */
    
    /// <summary>
    /// Collection các Problems mà user này sở hữu
    /// Đại diện cho relationship: 1 User có nhiều Problems (One-to-Many)
    /// </summary>
    public ICollection<Problem> OwnedProblems { get; set; } = new List<Problem>();
}