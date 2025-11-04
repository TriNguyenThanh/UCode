using UserService.Domain.Entities;

namespace UserService.Application.Interfaces.Repositories;

public interface ITeacherRepository : IRepository<Teacher>
{
    Task<Teacher?> GetByEmployeeIdAsync(string employeeId);
    Task<bool> EmployeeIdExistsAsync(string employeeId);
    Task<List<Teacher>> GetTeachersByDepartmentAsync(string department);
    Task<Teacher?> GetTeacherWithClassesAsync(Guid teacherId);
}

