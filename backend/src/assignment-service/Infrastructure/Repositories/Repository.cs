using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using AssignmentService.Application.Interfaces.Repositories;
using AssignmentService.Infrastructure.EF;

namespace AssignmentService.Infrastructure.Repositories;

/// <summary>
/// Generic Repository Implementation với auto-save
/// </summary>
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AssignmentDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(AssignmentDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<List<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public virtual async Task<(List<T> Items, int Total)> GetPagedAsync(
        int page,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
    {
        IQueryable<T> query = _dbSet;

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        var total = await query.CountAsync();

        if (orderBy != null)
        {
            query = orderBy(query);
        }

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await _dbSet.AddRangeAsync(entities);
        await _context.SaveChangesAsync();
    }

    // ✅ Đổi sang async
    public virtual async Task<T> UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    // ✅ Đổi sang async
    public virtual async Task<bool> RemoveAsync(Guid id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity == null)
            return false;

        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        if (predicate == null)
        {
            return await _dbSet.CountAsync();
        }

        return await _dbSet.CountAsync(predicate);
    }

    public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    public Task<bool> RemoveRangeAsync(IEnumerable<Guid> ids)
    {
        throw new NotSupportedException(
            $"RemoveRangeAsync is not supported for {typeof(T).Name}. " +
            "Please implement in specific repository if needed."
        );
    }
}