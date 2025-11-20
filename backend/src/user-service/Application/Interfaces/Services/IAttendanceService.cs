using UserService.Application.DTOs.Common;
using UserService.Application.DTOs.Requests;
using UserService.Application.DTOs.Responses;
using UserService.Domain.Entities;

public interface IAttendanceService
{
    Task<ApiResponse<List<AttendanceSessionResponse>>> GetSessionsAsync(Guid classId, int pageNumber, int pageSize);
    Task<ApiResponse<AttendanceSessionResponse?>> GetSessionByIdAsync(Guid id);
    Task<ApiResponse<AttendanceSessionResponse>> CreateSessionAsync(AttendanceSessionRequest request);
    Task<ApiResponse<AttendanceSessionResponse>> UpdateSessionAsync(AttendanceSessionRequest request);
    Task<ApiResponse<bool>> DeleteSessionAsync(Guid id);
    Task<ApiResponse<AttendanceRecordResponse>> CheckInAsync(AttendanceRecordRequest request);
    Task<ApiResponse<List<AttendanceRecordResponse>>> GetRecordsBySessionAsync(Guid sessionId, int pageNumber, int pageSize);
    Task<ApiResponse<AttendanceRecordResponse?>> GetRecordBySessionAndUserAsync(Guid sessionId, Guid userId);
    Task<ApiResponse<int>> GetTotalRecordsBySessionAsync(Guid sessionId);
}