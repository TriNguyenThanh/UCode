using System.ComponentModel.DataAnnotations;

namespace UserService.Application.DTOs.Requests;

/// <summary>
/// Request để tạo nhiều sinh viên cùng lúc (bulk create)
/// </summary>
public class BulkCreateStudentsRequest
{
    /// <summary>
    /// Danh sách sinh viên cần tạo
    /// </summary>
    [Required]
    [MinLength(1, ErrorMessage = "Phải có ít nhất 1 sinh viên")]
    public List<CreateStudentRequest> Students { get; set; } = new();
}
