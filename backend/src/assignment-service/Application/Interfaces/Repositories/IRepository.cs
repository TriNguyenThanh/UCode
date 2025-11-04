using System.Linq.Expressions;

namespace AssignmentService.Application.Interfaces.Repositories;

/// <summary>
/// Generic Repository Interface
/// </summary>
public interface IRepository<T> where T : class
{
    // Query
    Task<T?> GetByIdAsync(Guid id);
    Task<List<T>> GetAllAsync();
    Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);
    
    // Pagination
    Task<(List<T> Items, int Total)> GetPagedAsync(
        int page, 
        int pageSize, 
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);
    
    Task<T> AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    Task<T> UpdateAsync(T entity);
    Task<bool> RemoveAsync(Guid id);
    Task<bool> RemoveRangeAsync(IEnumerable<Guid> ids);

    // Aggregation
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
}