using UserService.Domain.Entities;

namespace UserService.Application.Interfaces.Repositories;

public interface IAttendanceRepository
{
    Task<List<AttendanceSession>> GetSessionsAsync(Guid classId, int pageNumber, int pageSize);
    Task<AttendanceSession?> GetSessionByIdAsync(Guid id);
    Task<AttendanceSession> CreateSessionAsync(AttendanceSession attendanceSession);
    Task<AttendanceSession> UpdateSessionAsync(AttendanceSession attendanceSession);
    Task<bool> DeleteSessionAsync(Guid id);
    Task<AttendanceRecord> CheckInAsync(AttendanceRecord attendanceRecord);
    Task<List<AttendanceRecord>> GetRecordsBySessionAsync(Guid sessionId, int pageNumber, int pageSize);
    Task<AttendanceRecord?> GetRecordBySessionAndUserAsync(Guid sessionId, Guid userId);
    Task<int> GetTotalRecordsBySessionAsync(Guid sessionId);
}