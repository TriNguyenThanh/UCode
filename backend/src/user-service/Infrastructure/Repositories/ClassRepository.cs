using Microsoft.EntityFrameworkCore;
using UserService.Application.Interfaces.Repositories;
using UserService.Domain.Entities;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public class ClassRepository : Repository<Class>, IClassRepository
{
    public ClassRepository(UserDbContext context) : base(context)
    {
    }

    public async Task<Class?> GetByClassCodeAsync(string classCode)
    {
        return await _dbSet
            .Include(c => c.Teacher)
            .Include(c => c.UserClasses)
                .ThenInclude(uc => uc.Student)
            .FirstOrDefaultAsync(c => c.ClassCode == classCode);
    }

    public async Task<bool> ClassCodeExistsAsync(string classCode)
    {
        return await _dbSet.AnyAsync(c => c.ClassCode == classCode);
    }

    public async Task<List<Class>> GetClassesByTeacherIdAsync(Guid teacherId)
    {
        return await _dbSet
            .Include(c => c.UserClasses)
            .Where(c => c.TeacherId == teacherId)
            .ToListAsync();
    }

    public async Task<List<Class>> GetClassesByStudentIdAsync(Guid studentId)
    {
        return await _dbSet
            .Include(c => c.Teacher)
            .Include(c => c.UserClasses)
            .Where(c => c.UserClasses.Any(uc => uc.StudentId == studentId && uc.IsActive))
            .ToListAsync();
    }

    public async Task<Class?> GetClassWithStudentsAsync(Guid classId)
    {
        return await _dbSet
            .Include(c => c.Teacher)
            .Include(c => c.UserClasses)
                .ThenInclude(uc => uc.Student)
            .FirstOrDefaultAsync(c => c.ClassId == classId);
    }

    public async Task<Class?> GetClassWithTeacherAsync(Guid classId)
    {
        return await _dbSet
            .Include(c => c.Teacher)
            .Include(c => c.UserClasses)
            .FirstOrDefaultAsync(c => c.ClassId == classId);
    }
}

