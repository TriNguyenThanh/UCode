using Microsoft.EntityFrameworkCore;
using UserService.Application.Interfaces.Repositories;
using UserService.Domain.Entities;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public class TeacherRepository : Repository<Teacher>, ITeacherRepository
{
    public TeacherRepository(UserDbContext context) : base(context)
    {
    }

    public async Task<Teacher?> GetByTeacherCodeAsync(string teacherCode)
    {
        return await _dbSet
            .Include(t => t.Classes)
            .FirstOrDefaultAsync(t => t.TeacherCode == teacherCode);
    }

    public async Task<bool> TeacherCodeExistsAsync(string teacherCode)
    {
        return await _dbSet.AnyAsync(t => t.TeacherCode == teacherCode);
    }

    public async Task<List<Teacher>> GetTeachersByDepartmentAsync(string department)
    {
        return await _dbSet
            .Where(t => t.Department.ToLower() == department.ToLower())
            .ToListAsync();
    }

    public async Task<Teacher?> GetTeacherWithClassesAsync(Guid teacherId)
    {
        return await _dbSet
            .Include(t => t.Classes)
                .ThenInclude(c => c.UserClasses)
                    .ThenInclude(uc => uc.Student)
            .FirstOrDefaultAsync(t => t.UserId == teacherId);
    }
}

