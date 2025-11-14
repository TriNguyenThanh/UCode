using UserService.Application.DTOs.Common;
using UserService.Application.DTOs.Requests;
using UserService.Application.DTOs.Responses;

namespace UserService.Application.Interfaces.Services;

public interface IStudentService
{
    Task<StudentResponse> CreateStudentAsync(CreateStudentRequest request);
    
    /// <summary>
    /// Tạo nhiều sinh viên cùng lúc (bulk create - optimized)
    /// </summary>
    Task<BulkCreateResult> BulkCreateStudentsAsync(List<CreateStudentRequest> students);
    
    Task<StudentResponse?> GetStudentByIdAsync(string studentId);
    Task<StudentResponse?> GetStudentByStudentCodeAsync(string studentCode);
    
    /// <summary>
    /// Lấy danh sách sinh viên với các filter (không bao gồm classes)
    /// </summary>
    Task<PagedResultDto<StudentListResponse>> GetStudentsAsync(
        int pageNumber, 
        int pageSize, 
        string? classId = null,
        string? search = null, 
        int? year = null, 
        string? major = null, 
        string? status = null, 
        string? excludeClassId = null);
    
    Task<List<StudentResponse>> GetStudentsByClassIdAsync(string classId);
    Task<bool> UpdateStudentAsync(string userId, UpdateUserRequest request);
    Task<bool> DeleteStudentAsync(string userId);
    
    /// <summary>
    /// Validate nhiều sinh viên cùng lúc theo mã sinh viên (optimized bulk validation)
    /// </summary>
    Task<List<BulkValidationResult>> ValidateBulkAsync(List<string> studentCodes);
    
    /// <summary>
    /// Validate batch students trước khi import
    /// </summary>
    Task<List<StudentValidationResult>> ValidateBatchAsync(List<string> identifiers, string? classId = null);
}

public class StudentValidationResult
{
    public string Identifier { get; set; } = string.Empty; // MSSV or Email
    public bool IsValid { get; set; }
    public string? StudentId { get; set; }
    public string? StudentCode { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsDuplicate { get; set; } // Đã enroll trong class
}
