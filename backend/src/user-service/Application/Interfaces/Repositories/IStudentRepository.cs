using UserService.Domain.Entities;

namespace UserService.Application.Interfaces.Repositories;

public interface IStudentRepository : IRepository<Student>
{
    Task<Student?> GetByStudentCodeAsync(string studentCode);
    Task<Student?> GetByEmailAsync(string email);
    Task<List<Student>> GetStudentsByClassIdAsync(Guid classId);
    Task<bool> StudentCodeExistsAsync(string studentCode);
    Task<List<Student>> GetStudentsByClassYearAsync(int classYear);
    Task<List<Student>> GetStudentsByMajorAsync(string major);
}
