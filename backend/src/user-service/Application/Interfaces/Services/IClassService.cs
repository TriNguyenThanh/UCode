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
    Task<List<StudentListResponse>> GetStudentListByClassAsync(string classId);
    
    /// <summary>
    /// Check duplicate students trong class
    /// </summary>
    Task<List<string>> CheckDuplicatesAsync(string classId, List<string> identifiers);
    
    /// <summary>
    /// Bulk enroll students vào class
    /// </summary>
    Task<BulkEnrollResult> BulkEnrollStudentsAsync(string classId, List<string> studentIds);

    // ===== ADMIN METHODS =====
    
    /// <summary>
    /// [ADMIN] Lấy tất cả lớp học với thông tin chi tiết cho admin
    /// </summary>
    Task<PagedResultDto<AdminClassResponse>> GetAllClassesForAdminAsync(
        int pageNumber, 
        int pageSize, 
        string? teacherId = null, 
        bool? isActive = null,
        bool? isArchived = null,
        string? searchTerm = null);
    
    /// <summary>
    /// [ADMIN] Lấy thông tin chi tiết lớp học cho admin
    /// </summary>
    Task<AdminClassResponse?> GetClassDetailForAdminAsync(string classId);
    
    /// <summary>
    /// [ADMIN] Archive lớp học
    /// </summary>
    Task<bool> ArchiveClassAsync(string classId, string? reason = null);
    
    /// <summary>
    /// [ADMIN] Unarchive lớp học
    /// </summary>
    Task<bool> UnarchiveClassAsync(string classId);
    
    /// <summary>
    /// [ADMIN] Cập nhật thông tin lớp học
    /// </summary>
    Task<bool> UpdateClassByAdminAsync(UpdateClassByAdminRequest request);
    
    /// <summary>
    /// [ADMIN] Xóa vĩnh viễn lớp học (chỉ admin)
    /// </summary>
    Task<bool> DeleteClassByAdminAsync(string classId);
    
    /// <summary>
    /// [ADMIN] Lấy thống kê tổng quan về classes
    /// </summary>
    Task<ClassStatisticsResponse> GetClassStatisticsAsync();
    
    /// <summary>
    /// [ADMIN] Bulk actions trên nhiều classes
    /// </summary>
    Task<object> BulkActionAsync(string action, List<string> classIds, string? reason);
    
    /// <summary>
    /// [ADMIN] Lấy danh sách students trong một class
    /// </summary>
    Task<object> GetClassStudentsForAdminAsync(Guid classId, int pageNumber, int pageSize, string? searchTerm);
}
