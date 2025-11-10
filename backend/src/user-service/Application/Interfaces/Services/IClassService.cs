using UserService.Application.DTOs.Common;
using UserService.Application.DTOs.Requests;
using UserService.Application.DTOs.Responses;

namespace UserService.Application.Interfaces.Services;

public interface IClassService
{
    Task<ClassResponse> CreateClassAsync(CreateClassRequest request);
    Task<ClassResponse?> GetClassByIdAsync(string classId);
    Task<ClassDetailResponse?> GetClassDetailAsync(string classId);
    Task<PagedResultDto<ClassResponse>> GetClassesAsync(int pageNumber, int pageSize, string? teacherId = null, bool? isActive = null);
    Task<List<ClassResponse>> GetClassesByTeacherIdAsync(string teacherId);
    Task<List<ClassResponse>> GetClassesByStudentIdAsync(string studentId); // New: Get enrolled classes for student
    Task<bool> UpdateClassAsync(string classId, UpdateClassRequest request);
    Task<bool> DeleteClassAsync(string classId);
    Task<bool> AddStudentToClassAsync(string classId, string studentId);
    Task<bool> AddStudentsToClassAsync(AddStudentsToClassRequest request);
    Task<bool> RemoveStudentFromClassAsync(string classId, string studentId);
    Task<List<StudentResponse>> GetStudentListByClassAsync(string classId);
    
    /// <summary>
    /// Check duplicate students trong class
    /// </summary>
    Task<List<string>> CheckDuplicatesAsync(string classId, List<string> identifiers);
    
    /// <summary>
    /// Bulk enroll students vào class
    /// </summary>
    Task<BulkEnrollResult> BulkEnrollStudentsAsync(string classId, List<string> studentIds);
}

public class BulkEnrollResult
{
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<string> SuccessIds { get; set; } = new();
    public List<BulkEnrollError> Errors { get; set; } = new();
}

public class BulkEnrollError
{
    public string StudentId { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}
