using UserService.Application.DTOs.Common;
using UserService.Application.DTOs.Requests;
using UserService.Application.DTOs.Responses;

namespace UserService.Application.Interfaces.Services;

public interface IStudentService
{
    Task<StudentResponse> CreateStudentAsync(CreateStudentRequest request);
    Task<StudentResponse?> GetStudentByIdAsync(string studentId);
    Task<StudentResponse?> GetStudentByStudentCodeAsync(string studentCode);
    Task<PagedResultDto<StudentResponse>> GetStudentsAsync(int pageNumber, int pageSize, string? classId = null);
    Task<List<StudentResponse>> GetStudentsByClassIdAsync(string classId);
    Task<bool> UpdateStudentAsync(string userId, UpdateUserRequest request);
    Task<bool> DeleteStudentAsync(string userId);
}

