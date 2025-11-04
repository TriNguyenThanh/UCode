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
            .Include(p => p.ProblemLanguages)
                .ThenInclude(pl => pl.Language)
            .Include(p => p.ProblemAssets)
            .Include(p => p.ProblemTags)
                .ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.ProblemId == problemId);
    }

    public async Task<List<Problem>> GetByOwnerIdAsync(Guid ownerId)
    {
        return await _context.Problems
            .AsNoTracking()
            .Where(p => p.OwnerId == ownerId)
            .ToListAsync();
    }

    public async Task<Problem?> GetBySlugAsync(string slug)
    {
        return await _context.Problems
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Slug == slug);
    }

    public async Task<List<Problem>> GetByTagNameAsync(string tagName)
    {
        return await _context.Problems
            .AsNoTracking()
            .Where(p => p.ProblemTags.Any(pt => Microsoft.EntityFrameworkCore.EF.Functions.Like(pt.Tag.Name, tagName)))
            .ToListAsync();
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

    public async Task<(List<Problem> problems, int total)> SearchProblemsAsync(string? keyword, string? difficulty, int page, int pageSize)
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

        var total = await query.CountAsync();
        var problems = await query
            .OrderByDescending(p => p.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (problems, total);
    }

    public async Task<Problem> UpdateAsync(Problem entity)
    {
        var existingProblem = await _context.Problems
            .Include(p => p.ProblemAssets)
            .Include(p => p.Datasets)
                .ThenInclude(d => d.TestCases)
            .Include(p => p.ProblemLanguages)
                .ThenInclude(pl => pl.Language)
            .Include(p => p.ProblemTags)
            .FirstOrDefaultAsync(p => p.ProblemId == entity.ProblemId);
            
        if (existingProblem == null)
            throw new KeyNotFoundException("Problem not found");
        
        // Update scalar properties
        _context.Entry(existingProblem).CurrentValues.SetValues(entity);
        
        // Handle ProblemAssets updates
        if (entity.ProblemAssets != null)
        {
            // Remove existing assets
            existingProblem.ProblemAssets.Clear();
            
            // Add new assets
            foreach (var asset in entity.ProblemAssets)
            {
                existingProblem.ProblemAssets.Add(asset);
            }
        }
        
        await _context.SaveChangesAsync();
        
        // Reload with full details
        return await GetProblemWithDetailsAsync(existingProblem.ProblemId) ?? existingProblem;
    }

    public Task<string> GetNextCodeSequenceAsync()
    {
        var result = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                      .Replace("/", "").Replace("+", "").Replace("=", "")
                      .Substring(0, 5)
                      .ToUpperInvariant();
        return Task.FromResult(result);
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

    // Pagination
    public async Task<(List<Problem> problems, int total)> GetByOwnerIdWithPaginationAsync(Guid ownerId, int page, int pageSize)
    {
        var query = _context.Problems
            .Where(p => p.OwnerId == ownerId)
            .OrderByDescending(p => p.UpdatedAt);

        var total = await query.CountAsync();
        var problems = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (problems, total);
    }

    // ProblemAsset methods
    public async Task<List<ProblemAsset>> GetProblemAssetsAsync(Guid problemId)
    {
        return await _context.ProblemAssets
            .Where(pa => pa.ProblemId == problemId)
            .OrderBy(pa => pa.OrderIndex)
            .ToListAsync();
    }

    public async Task<ProblemAsset> AddProblemAssetAsync(ProblemAsset asset)
    {
        await _context.ProblemAssets.AddAsync(asset);
        await _context.SaveChangesAsync();
        return asset;
    }

    public async Task<ProblemAsset?> GetProblemAssetByIdAsync(Guid assetId)
    {
        return await _context.ProblemAssets.FindAsync(assetId);
    }

    public async Task<bool> UpdateProblemAssetAsync(ProblemAsset asset)
    {
        _context.ProblemAssets.Update(asset);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteProblemAssetAsync(Guid assetId)
    {
        var asset = await _context.ProblemAssets.FindAsync(assetId);
        if (asset == null) return false;

        _context.ProblemAssets.Remove(asset);
        await _context.SaveChangesAsync();
        return true;
    }

    // Tag methods
    public async Task<List<Tag>> GetAllTagsAsync()
    {
        return await _context.Tags.OrderBy(t => t.Name).ToListAsync();
    }

    public async Task AddProblemTagsAsync(Guid problemId, List<Guid> tagIds)
    {
        var existingTags = await _context.ProblemTags
            .Where(pt => pt.ProblemId == problemId)
            .Select(pt => pt.TagId)
            .ToListAsync();

        var newTagIds = tagIds.Except(existingTags).ToList();

        var problemTags = newTagIds.Select(tagId => new ProblemTag
        {
            ProblemId = problemId,
            TagId = tagId
        }).ToList();

        await _context.ProblemTags.AddRangeAsync(problemTags);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveProblemTagAsync(Guid problemId, Guid tagId)
    {
        var problemTag = await _context.ProblemTags
            .FirstOrDefaultAsync(pt => pt.ProblemId == problemId && pt.TagId == tagId);

        if (problemTag != null)
        {
            _context.ProblemTags.Remove(problemTag);
            await _context.SaveChangesAsync();
        }
    }

    // ProblemLanguage methods (using new Language + ProblemLanguage schema with composite PK)
    public async Task<Problem?> GetByIdWithLanguagesAsync(Guid problemId)
    {
        return await _context.Problems
            // .AsNoTracking()
            .Include(p => p.ProblemLanguages)
                .ThenInclude(pl => pl.Language)
            .FirstOrDefaultAsync(p => p.ProblemId == problemId);
    }

    public async Task<List<ProblemLanguage>> GetProblemLanguagesAsync(Guid problemId)
    {
        return await _context.ProblemLanguages
            .Include(pl => pl.Language) // Include global language config
            .Where(pl => pl.ProblemId == problemId)
            .OrderBy(pl => pl.Language.DisplayOrder)
            .ToListAsync();
    }

    public async Task<ProblemLanguage?> GetProblemLanguageAsync(Guid problemId, Guid languageId)
    {
        return await _context.ProblemLanguages
            .Include(pl => pl.Language)
            .FirstOrDefaultAsync(pl => pl.ProblemId == problemId && pl.LanguageId == languageId);
    }

    public async Task<ProblemLanguage> AddProblemLanguageAsync(ProblemLanguage problemLanguage)
    {
        await _context.ProblemLanguages.AddAsync(problemLanguage);
        await _context.SaveChangesAsync();
        
        // Reload with Language navigation
        return await GetProblemLanguageAsync(problemLanguage.ProblemId, problemLanguage.LanguageId) 
            ?? problemLanguage;
    }

    public async Task<bool> UpdateProblemLanguageAsync(ProblemLanguage problemLanguage)
    {
        _context.ProblemLanguages.Update(problemLanguage);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteProblemLanguageAsync(Guid problemId, Guid languageId)
    {
        var problemLanguage = await _context.ProblemLanguages
            .FindAsync(problemId, languageId); // Composite key

        if (problemLanguage == null) return false;

        _context.ProblemLanguages.Remove(problemLanguage);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}