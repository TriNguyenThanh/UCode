using System.ComponentModel.DataAnnotations;

namespace UserService.Application.DTOs.Requests;

/// <summary>
/// Request DTO cho Admin cập nhật thông tin lớp học
/// </summary>
public class UpdateClassByAdminRequest
{
    [Required(ErrorMessage = "ClassId is required")]
    public Guid ClassId { get; set; }
    
    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 200 characters")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string Description { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "TeacherId is required")]
    public Guid TeacherId { get; set; }
    
    public bool IsActive { get; set; }
}
