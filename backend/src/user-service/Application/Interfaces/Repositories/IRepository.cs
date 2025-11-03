using System.Linq.Expressions;

namespace UserService.Application.Interfaces.Repositories;

/// <summary>
/// Generic repository interface
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<List<T>> GetAllAsync();
    Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task<bool> UpdateAsync(T entity);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    Task<List<T>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<T, bool>>? filter = null);
}

