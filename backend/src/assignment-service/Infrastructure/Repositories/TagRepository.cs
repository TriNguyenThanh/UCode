using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using AssignmentService.Application.Interfaces.Repositories;
using AssignmentService.Domain.Entities;
using AssignmentService.Infrastructure.EF;

namespace AssignmentService.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Tag entity
/// </summary>
public class TagRepository : ITagRepository
{
    private readonly AssignmentDbContext _context;

    public TagRepository(AssignmentDbContext context)
    {
        _context = context;
    }

    public async Task<Tag> AddAsync(Tag entity)
    {
        await _context.Tags.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task AddRangeAsync(IEnumerable<Tag> entities)
    {
        await _context.Tags.AddRangeAsync(entities);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> AnyAsync(Expression<Func<Tag, bool>> predicate)
    {
        return await _context.Tags.AnyAsync(predicate);
    }

    public async Task<int> CountAsync(Expression<Func<Tag, bool>>? predicate = null)
    {
        IQueryable<Tag> query = _context.Tags;

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        return await query.CountAsync();
    }

    public async Task<List<Tag>> FindAsync(Expression<Func<Tag, bool>> predicate)
    {
        return await _context.Tags
            .AsNoTracking()
            .Where(predicate)
            .ToListAsync();
    }

    public async Task<List<Tag>> GetAllAsync()
    {
        return await _context.Tags
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<Tag?> GetByIdAsync(Guid id)
    {
        return await _context.Tags
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TagId == id);
    }

    public async Task<bool> RemoveAsync(Guid id)
    {
        var tag = await _context.Tags.FindAsync(id);
        if (tag == null) return false;

        _context.Tags.Remove(tag);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveRangeAsync(IEnumerable<Guid> ids)
    {
        var tags = await _context.Tags
            .Where(t => ids.Contains(t.TagId))
            .ToListAsync();

        if (!tags.Any()) return false;

        _context.Tags.RemoveRange(tags);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Tag> UpdateAsync(Tag entity)
    {
        _context.Tags.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<(List<Tag> Items, int Total)> GetPagedAsync(
        int page, 
        int pageSize, 
        Expression<Func<Tag, bool>>? predicate = null, 
        Func<IQueryable<Tag>, IOrderedQueryable<Tag>>? orderBy = null)
    {
        IQueryable<Tag> query = _context.Tags.AsNoTracking();

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        var total = await query.CountAsync();

        if (orderBy != null)
        {
            query = orderBy(query);
        }
        else
        {
            query = query.OrderBy(t => t.Name);
        }

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<List<Problem>> GetProblemsByTagIdAsync(Guid tagId)
    {
        return await _context.ProblemTags
            .AsNoTracking()
            .Where(pt => pt.TagId == tagId)
            .Include(pt => pt.Problem)
            .Select(pt => pt.Problem)
            .ToListAsync();
    }
}
