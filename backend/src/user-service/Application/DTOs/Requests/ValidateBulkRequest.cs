using System.ComponentModel.DataAnnotations;

namespace UserService.Application.DTOs.Requests;

/// <summary>
/// Request để validate nhiều sinh viên cùng lúc
/// </summary>
public class ValidateBulkRequest
{
    /// <summary>
    /// Danh sách mã sinh viên cần kiểm tra
    /// </summary>
    [Required(ErrorMessage = "StudentCodes is required")]
    [MinLength(1, ErrorMessage = "At least one student code is required")]
    public List<string> StudentCodes { get; set; } = new();
}
