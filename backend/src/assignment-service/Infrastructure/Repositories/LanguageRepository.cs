using Microsoft.EntityFrameworkCore;
using AssignmentService.Application.Interfaces.Repositories;
using AssignmentService.Domain.Entities;
using AssignmentService.Infrastructure.EF;
using System.Linq.Expressions;

namespace AssignmentService.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Language entity
/// </summary>
public class LanguageRepository : ILanguageRepository
{
    private readonly AssignmentDbContext _context;

    public LanguageRepository(AssignmentDbContext context)
    {
        _context = context;
    }

    #region IRepository<Language> Base Methods

    public async Task<Language> AddAsync(Language entity)
    {
        await _context.Languages.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task AddRangeAsync(IEnumerable<Language> entities)
    {
        await _context.Languages.AddRangeAsync(entities);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> AnyAsync(Expression<Func<Language, bool>> predicate)
    {
        return await _context.Languages.AnyAsync(predicate);
    }

    public async Task<int> CountAsync(Expression<Func<Language, bool>>? predicate = null)
    {
        IQueryable<Language> query = _context.Languages;

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        return await query.CountAsync();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var language = await _context.Languages.FindAsync(id);
        if (language == null) return false;

        _context.Languages.Remove(language);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Language>> FindAsync(Expression<Func<Language, bool>> predicate)
    {
        return await _context.Languages.Where(predicate).ToListAsync();
    }

    public async Task<List<Language>> GetAllAsync()
    {
        return await _context.Languages
            .OrderBy(l => l.DisplayOrder)
            .ToListAsync();
    }

    public async Task<Language?> GetByIdAsync(Guid id)
    {
        return await _context.Languages.FindAsync(id);
    }

    public async Task<(List<Language> Items, int Total)> GetPagedAsync(
        int page, 
        int pageSize, 
        Expression<Func<Language, bool>>? predicate = null, 
        Func<IQueryable<Language>, IOrderedQueryable<Language>>? orderBy = null)
    {
        var query = _context.Languages.AsQueryable();

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
            query = query.OrderBy(l => l.DisplayOrder);
        }

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<bool> RemoveAsync(Guid id)
    {
        return await DeleteAsync(id);
    }

    public async Task<bool> RemoveRangeAsync(IEnumerable<Guid> ids)
    {
        var languages = await _context.Languages
            .Where(l => ids.Contains(l.LanguageId))
            .ToListAsync();
        
        if (languages.Count == 0) return false;

        _context.Languages.RemoveRange(languages);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Language> UpdateAsync(Language entity)
    {
        _context.Languages.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    #endregion

    #region ILanguageRepository Specific Methods

    public async Task<List<Language>> GetAllLanguagesAsync(bool includeDisabled = false)
    {
        var query = _context.Languages.AsQueryable();

        if (!includeDisabled)
        {
            query = query.Where(l => l.IsEnabled);
        }

        return await query
            .OrderBy(l => l.DisplayOrder)
            .ToListAsync();
    }

    public async Task<Language?> GetLanguageByCodeAsync(string code)
    {
        return await _context.Languages
            .FirstOrDefaultAsync(l => l.Code == code);
    }

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeLanguageId = null)
    {
        var query = _context.Languages.Where(l => l.Code == code);

        if (excludeLanguageId.HasValue)
        {
            query = query.Where(l => l.LanguageId != excludeLanguageId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<bool> SetLanguageEnabledAsync(Guid languageId, bool isEnabled)
    {
        var language = await _context.Languages.FindAsync(languageId);
        if (language == null) return false;

        language.IsEnabled = isEnabled;
        await _context.SaveChangesAsync();
        return true;
    }

    #endregion
}
