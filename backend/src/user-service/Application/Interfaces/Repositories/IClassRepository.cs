using UserService.Domain.Entities;

namespace UserService.Application.Interfaces.Repositories;

public interface IClassRepository : IRepository<Class>
{
    Task<Class?> GetByClassCodeAsync(string classCode);
    Task<bool> ClassCodeExistsAsync(string classCode);
    Task<List<Class>> GetClassesByTeacherIdAsync(Guid teacherId);
    Task<List<Class>> GetClassesByStudentIdAsync(Guid studentId); // New: Get enrolled classes
    Task<Class?> GetClassWithStudentsAsync(Guid classId);
    Task<Class?> GetClassWithTeacherAsync(Guid classId);
}