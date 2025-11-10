using UserService.Domain.Entities;

namespace UserService.Application.Interfaces.Repositories;

public interface IUserClassRepository
{
    Task<UserClass?> GetByIdsAsync(Guid studentId, Guid classId);
    Task<List<UserClass>> GetByStudentIdAsync(Guid studentId);
    Task<List<UserClass>> GetByClassIdAsync(Guid classId);
    Task<UserClass> AddAsync(UserClass userClass);
    Task<bool> RemoveAsync(Guid studentId, Guid classId);
    Task<bool> ExistsAsync(Guid studentId, Guid classId);
    Task<bool> IsStudentEnrolledAsync(Guid studentId, Guid classId);
    Task<int> CountStudentsInClassAsync(Guid classId);
    Task<List<UserClass>> AddRangeAsync(List<UserClass> userClasses);
}
