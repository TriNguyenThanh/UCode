using UserService.Domain.Entities;

namespace UserService.Application.Interfaces.Repositories;

public interface ITeacherRepository : IRepository<Teacher>
{
    Task<Teacher?> GetByTeacherCodeAsync(string teacherCode);
    Task<bool> TeacherCodeExistsAsync(string teacherCode);
    Task<List<Teacher>> GetTeachersByDepartmentAsync(string department);
    Task<Teacher?> GetTeacherWithClassesAsync(Guid teacherId);
}

