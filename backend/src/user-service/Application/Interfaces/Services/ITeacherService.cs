using UserService.Application.DTOs.Common;
using UserService.Application.DTOs.Requests;
using UserService.Application.DTOs.Responses;

namespace UserService.Application.Interfaces.Services;

public interface ITeacherService
{
    Task<TeacherResponse> CreateTeacherAsync(CreateTeacherRequest request);
    Task<TeacherResponse?> GetTeacherByIdAsync(string teacherId);
    Task<TeacherResponse?> GetTeacherByEmployeeIdAsync(string employeeId);
    Task<PagedResultDto<TeacherResponse>> GetTeachersAsync(int pageNumber, int pageSize, string? department = null);
    Task<List<ClassResponse>> GetTeacherClassesAsync(string teacherId);
    Task<bool> UpdateTeacherAsync(string userId, UpdateUserRequest request);
    Task<bool> DeleteTeacherAsync(string userId);
}

