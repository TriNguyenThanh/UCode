using UserService.Domain.Entities;

namespace UserService.Application.Interfaces.Repositories;

public interface IStudentRepository : IRepository<Student>
{
    Task<Student?> GetByStudentIdAsync(string studentId);
    Task<List<Student>> GetStudentsByClassIdAsync(Guid classId);
    Task<bool> StudentIdExistsAsync(string studentId);
    Task<List<Student>> GetStudentsByClassYearAsync(int classYear);
    Task<List<Student>> GetStudentsByMajorAsync(string major);
}

