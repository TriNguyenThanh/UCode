using UserService.Application.Interfaces.Repositories;
using UserService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public class AttendanceRepository : IAttendanceRepository
{
    private readonly UserDbContext _context;

    public AttendanceRepository(UserDbContext context)
    {
        _context = context;
    }

    public async Task<AttendanceRecord> CheckInAsync(AttendanceRecord attendanceRecord)
    {
        try
        {
            await _context.AttendanceRecords.AddAsync(attendanceRecord);
            await _context.SaveChangesAsync();
            return attendanceRecord;
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while checking in.", ex);
        }
    }

    public async Task<AttendanceSession> CreateSessionAsync(AttendanceSession attendanceSession)
    {
        try
        {
            await _context.AttendanceSessions.AddAsync(attendanceSession);
            await _context.SaveChangesAsync();
            return attendanceSession;
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while creating the session.", ex);
        }
    }

    public async Task<bool> DeleteSessionAsync(Guid id)
    {
        try
        {
            var session = await _context.AttendanceSessions.FindAsync(id);
            if (session == null)
                return false;

            _context.AttendanceSessions.Remove(session);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while deleting the session.", ex);
        }
    }

    public async Task<AttendanceRecord?> GetRecordBySessionAndUserAsync(Guid sessionId, Guid userId)
    {
        return await _context.AttendanceRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(record => record.SessionId == sessionId && record.UserId == userId);
    }

    public async Task<List<AttendanceRecord>> GetRecordsBySessionAsync(Guid sessionId, int pageNumber, int pageSize)
    {
        return await _context.AttendanceRecords
            .AsNoTracking()
            .Where(record => record.SessionId == sessionId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<AttendanceSession?> GetSessionByIdAsync(Guid id)
    {
        return await _context.AttendanceSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(session => session.Id == id);
    }

    public async Task<List<AttendanceSession>> GetSessionsAsync(Guid classId, int pageNumber, int pageSize)
    {
        return await _context.AttendanceSessions
            .AsNoTracking()
            .Where(session => session.ClassId == classId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalRecordsBySessionAsync(Guid sessionId)
    {
        return await _context.AttendanceRecords.CountAsync(record => record.SessionId == sessionId);
    }

    public async Task<AttendanceSession> UpdateSessionAsync(AttendanceSession attendanceSession)
    {
        try
        {        
            _context.AttendanceSessions.Update(attendanceSession);
            await _context.SaveChangesAsync();
            return attendanceSession;
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while updating the session.", ex);
        }
    }
}