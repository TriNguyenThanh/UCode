using Microsoft.EntityFrameworkCore;
using UserService.Application.Interfaces.Repositories;
using UserService.Domain.Entities;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public class UserClassRepository : IUserClassRepository
{
    private readonly UserDbContext _context;
    private readonly DbSet<UserClass> _dbSet;

    public UserClassRepository(UserDbContext context)
    {
        _context = context;
        _dbSet = context.Set<UserClass>();
    }

    public async Task<UserClass?> GetByIdsAsync(Guid studentId, Guid classId)
    {
        return await _dbSet
            .Include(uc => uc.Student)
            .Include(uc => uc.Class)
            .FirstOrDefaultAsync(uc => uc.StudentId == studentId && uc.ClassId == classId);
    }

    public async Task<List<UserClass>> GetByStudentIdAsync(Guid studentId)
    {
        return await _dbSet
            .Include(uc => uc.Class)
                .ThenInclude(c => c.Teacher)
            .Where(uc => uc.StudentId == studentId)
            .ToListAsync();
    }

    public async Task<List<UserClass>> GetByClassIdAsync(Guid classId)
    {
        return await _dbSet
            .Include(uc => uc.Student)
            .Where(uc => uc.ClassId == classId)
            .ToListAsync();
    }

    public async Task<UserClass> AddAsync(UserClass userClass)
    {
        await _dbSet.AddAsync(userClass);
        await _context.SaveChangesAsync();
        return userClass;
    }

    public async Task<bool> RemoveAsync(Guid studentId, Guid classId)
    {
        var userClass = await GetByIdsAsync(studentId, classId);
        if (userClass == null)
            return false;

        _dbSet.Remove(userClass);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> ExistsAsync(Guid studentId, Guid classId)
    {
        return await _dbSet.AnyAsync(uc => uc.StudentId == studentId && uc.ClassId == classId);
    }

    public async Task<bool> IsStudentEnrolledAsync(Guid studentId, Guid classId)
    {
        return await _dbSet.AnyAsync(uc => 
            uc.StudentId == studentId && 
            uc.ClassId == classId && 
            uc.IsActive);
    }

    public async Task<int> CountStudentsInClassAsync(Guid classId)
    {
        return await _dbSet.CountAsync(uc => uc.ClassId == classId && uc.IsActive);
    }

    public async Task<List<UserClass>> AddRangeAsync(List<UserClass> userClasses)
    {
        await _dbSet.AddRangeAsync(userClasses);
        await _context.SaveChangesAsync();
        return userClasses;
    }
}
