using System.Drawing;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using AssignmentService.Application.Interfaces.Repositories;
using AssignmentService.Domain.Entities;
using AssignmentService.Domain.Enums;
using AssignmentService.Infrastructure.EF;

namespace AssignmentService.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Problem entity
///  </summary>

public class ProblemRepository : IProblemRepository
{
    private readonly AssignmentDbContext _context;

    public ProblemRepository(AssignmentDbContext _context)
    {
        this._context = _context;
    }

    public async Task<Problem> AddAsync(Problem entity)
    {
        await _context.Problems.AddAsync(entity);
        await _context.SaveChangesAsync();

        // Reload với đầy đủ navigations
        return await GetProblemWithDetailsAsync(entity.ProblemId) ?? entity;
    }

    public async Task AddRangeAsync(IEnumerable<Problem> entities)
    {
        await _context.Problems.AddRangeAsync(entities);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> AnyAsync(Expression<Func<Problem, bool>> predicate)
    {
        return await _context.Problems.AnyAsync(predicate);
    }

    public async Task<int> CountAsync(Expression<Func<Problem, bool>>? predicate = null)
    {
        IQueryable<Problem> query = _context.Problems;

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        return await query.CountAsync();
    }

    public Task<List<Problem>> FindAsync(Expression<Func<Problem, bool>> predicate)
    {
        return _context.Problems.AsNoTracking().Where(predicate).ToListAsync();
    }

    public Task<List<Problem>> GetAllAsync()
    {
        return _context.Problems.AsNoTracking().ToListAsync();
    }

    public async Task<Problem?> GetByCodeAsync(string code)
    {
        return await _context.Problems
            .FirstOrDefaultAsync(p => p.Code == code);
    }

    public async Task<Problem?> GetByIdAsync(Guid id)
    {
        var problem = await GetProblemWithDetailsAsync(id);
        return problem;
    }

    public Task<Problem?> GetByIdWithDetailsAsync(Guid id)
    {
        return GetProblemWithDetailsAsync(id);
    }

    // Helper method để load đầy đủ navigations
    private async Task<Problem?> GetProblemWithDetailsAsync(Guid problemId)
    {
        return await _context.Problems
            .AsSplitQuery()
            .Include(p => p.Datasets)
                .ThenInclude(d => d.TestCases)
            .Include(p => p.LanguageLimits)
            .Include(p => p.CodeTemplates)
            .Include(p => p.ProblemAssets)
            .Include(p => p.ProblemTags)
            .FirstOrDefaultAsync(p => p.ProblemId == problemId);
    }

    public async Task<List<Problem>> GetByOwnerIdAsync(Guid ownerId)
    {
        return await _context.Problems
            .AsNoTracking()
            .Where(p => p.OwnerId == ownerId)
            .ToListAsync();
    }

    public Task<Problem?> GetBySlugAsync(string slug)
    {
        throw new NotImplementedException();
    }

    public Task<List<Problem>> GetByTagNameAsync(string tagName)
    {
        throw new NotImplementedException();
    }

    public async Task<(List<Problem> Items, int Total)> GetPagedAsync(int page, int pageSize, Expression<Func<Problem, bool>>? predicate = null, Func<IQueryable<Problem>, IOrderedQueryable<Problem>>? orderBy = null)
    {
        IQueryable<Problem> query = _context.Problems;
        
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

    public async Task<List<Problem>> GetPublishedProblemsAsync(int page, int pageSize)
    {
        return await _context.Problems
            .AsNoTracking()
            .Where(p => p.IsLocked == false)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<bool> RemoveAsync(Guid id)
    {
        var problem = await _context.Problems.FindAsync(id);
        if (problem == null)
            throw new KeyNotFoundException("Problem not found");

        _context.Problems.Remove(problem);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveRangeAsync(IEnumerable<Guid> ids)
    {
        var entities = await _context.Problems
            .Where(p => ids.Contains(p.ProblemId))
            .ToListAsync();

        if (entities.Count == 0)
            return false;
        _context.Problems.RemoveRange(entities);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Problem>> SearchProblemsAsync(string? keyword, string? difficulty, int page, int pageSize)
    {
        var query = _context.Problems.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(p => (p.Title != null && p.Title.Contains(keyword)) || 
                                    (p.Description != null && p.Description.Contains(keyword)));
        }

        if (!string.IsNullOrEmpty(difficulty))
        {
            query = query.Where(p => p.Difficulty.ToString().ToLower() == difficulty.ToLower());
        }

        return await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
    }

    public async Task<Problem> UpdateAsync(Problem entity)
    {
        var existingProblem = await _context.Problems.FindAsync(entity.ProblemId);
        if (existingProblem == null)
            throw new KeyNotFoundException("Problem not found");
        _context.Entry(existingProblem).CurrentValues.SetValues(entity);
        await _context.SaveChangesAsync();
        return existingProblem;
    }

    public async Task<string> GetNextCodeSequenceAsync()
    {
        var result = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                      .Replace("/", "").Replace("+", "").Replace("=", "")
                      .Substring(0, 5)
                      .ToUpperInvariant();
        return result;
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeProblemId = null)
    {
        return await _context.Problems.AnyAsync(p => p.Slug == slug && p.ProblemId != excludeProblemId);
    }

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeProblemId = null)
    {
        return await _context.Problems.AnyAsync(p => p.Code == code && p.ProblemId != excludeProblemId);
    }
    
    public async Task<List<Dataset>> GetDatasetsByProblemIdAsync(Guid problemId)
    {
        var problem = await _context.Problems
            .AsSplitQuery()
            .Include(p => p.Datasets)
                .ThenInclude(d => d.TestCases)
            .FirstOrDefaultAsync(p => p.ProblemId == problemId);

        return problem?.Datasets.ToList() ?? new List<Dataset>();
    }

    public async Task<Guid?> GetProblemOwnerIdAsync(Guid problemId)
    {
        return await _context.Problems
            .Where(p => p.ProblemId == problemId)
            .Select(p => (Guid?)p.OwnerId)
            .FirstOrDefaultAsync();
    }

    public async Task<(bool exists, Guid? ownerId)> CheckExistsAndGetOwnerAsync(Guid problemId)
    {
        var result = await _context.Problems
            .Where(p => p.ProblemId == problemId)
            .Select(p => new { p.OwnerId })
            .FirstOrDefaultAsync();

        return result != null ? (true, result.OwnerId) : (false, null);
    }

    public async Task<(bool exists, Guid? ownerId, Visibility visibility)> GetProblemBasicInfoAsync(Guid problemId)
    {
        var result = await _context.Problems
            .Where(p => p.ProblemId == problemId)
            .Select(p => new { p.OwnerId, p.Visibility })
            .FirstOrDefaultAsync();

        if (result == null)
            return (false, null, Visibility.PRIVATE);

        return (true, result.OwnerId, result.Visibility);
    }
}