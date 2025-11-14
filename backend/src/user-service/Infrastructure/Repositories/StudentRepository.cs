using Microsoft.EntityFrameworkCore;
using UserService.Application.Interfaces.Repositories;
using UserService.Domain.Entities;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public class StudentRepository : Repository<Student>, IStudentRepository
{
    public StudentRepository(UserDbContext context) : base(context)
    {
    }

    public override async Task<Student?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(s => s.UserClasses)
                .ThenInclude(uc => uc.Class)
                    .ThenInclude(c => c.Teacher)
            .FirstOrDefaultAsync(s => s.UserId == id);
    }

    public async Task<Student?> GetByStudentCodeAsync(string studentCode)
    {
        return await _dbSet
            .Include(s => s.UserClasses)
                .ThenInclude(uc => uc.Class)
            .FirstOrDefaultAsync(s => s.StudentCode == studentCode);
    }

    public async Task<Student?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .Include(s => s.UserClasses)
                .ThenInclude(uc => uc.Class)
            .FirstOrDefaultAsync(s => s.Email == email);
    }

    public async Task<List<Student>> GetStudentsByClassIdAsync(Guid classId)
    {
        return await _dbSet
            .Include(s => s.UserClasses)
                .ThenInclude(uc => uc.Class)
            .Where(s => s.UserClasses.Any(uc => uc.ClassId == classId && uc.IsActive))
            .ToListAsync();
    }

    public async Task<bool> StudentCodeExistsAsync(string studentCode)
    {
        return await _dbSet.AnyAsync(s => s.StudentCode == studentCode);
    }

    public async Task<List<Student>> GetStudentsByClassYearAsync(int classYear)
    {
        return await _dbSet
            .Where(s => s.ClassYear == classYear)
            .ToListAsync();
    }

    public async Task<List<Student>> GetStudentsByMajorAsync(string major)
    {
        return await _dbSet
            .Where(s => s.Major.ToLower() == major.ToLower())
            .ToListAsync();
    }

    public async Task<List<Student>> GetByStudentCodesAsync(List<string> studentCodes)
    {
        return await _dbSet
            .Where(s => studentCodes.Contains(s.StudentCode))
            .ToListAsync();
    }
}

