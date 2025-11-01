using AssignmentService.Application.Interfaces.Repositories;
using AssignmentService.Domain.Entities;
using AssignmentService.Infrastructure.EF;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AssignmentService.Infrastructure.Repositories;

/// <summary>
/// Repository cho Dataset entity
/// </summary>
public class DatasetRepository : IDatasetRepository
{
    private readonly AssignmentDbContext context;

    public DatasetRepository(AssignmentDbContext context)
    {
        this.context = context;
    }

    public async Task<Dataset> AddAsync(Dataset entity)
    {
        await context.Datasets.AddAsync(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task AddRangeAsync(IEnumerable<Dataset> entities)
    {
        await context.Datasets.AddRangeAsync(entities);
        await context.SaveChangesAsync();
    }

    public async Task<bool> AnyAsync(Expression<Func<Dataset, bool>> predicate)
    {
        return await context.Datasets.AnyAsync(predicate);
    }

    public Task<int> CountAsync(Expression<Func<Dataset, bool>>? predicate = null)
    {
        throw new NotImplementedException();
    }

    public Task<List<Dataset>> FindAsync(Expression<Func<Dataset, bool>> predicate)
    {
        throw new NotImplementedException();
    }

    public async Task<List<Dataset>> GetAllAsync()
    {
        return await context.Datasets.AsNoTracking().ToListAsync();
    }

    public async Task<Dataset?> GetByIdAsync(Guid id)
    {
        return await context.Datasets.FirstOrDefaultAsync(d => d.DatasetId == id);
    }

    public async Task<Dataset?> GetByIdWithDetailsAsync(Guid id)
    {
        return await context.Datasets
            .AsSplitQuery()
            .Include(t => t.TestCases)
            .FirstOrDefaultAsync(d => d.DatasetId == id);
    }

    public async Task<List<Dataset>> GetByProblemIdAsync(Guid problemId)
    {
        return await context.Datasets
            .Include(d => d.TestCases)
            .Where(d => d.ProblemId == problemId)
            .ToListAsync();
    }

    public Task<(List<Dataset> Items, int Total)> GetPagedAsync(int page, int pageSize, Expression<Func<Dataset, bool>>? predicate = null, Func<IQueryable<Dataset>, IOrderedQueryable<Dataset>>? orderBy = null)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> RemoveAsync(Guid id)
    {
        var dataset = await context.Datasets.FindAsync(id);
        if (dataset == null)
            throw new KeyNotFoundException("Dataset not found");

        context.Datasets.Remove(dataset);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveRangeAsync(IEnumerable<Guid> ids)
    {
        var entities = await context.Datasets
            .Where(d => ids.Contains(d.DatasetId))
            .ToListAsync();

        if (entities.Count == 0)
            return false;
        context.Datasets.RemoveRange(entities);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<Dataset> UpdateAsync(Dataset entity)
    {
        // 1. Load dataset hiện tại từ database
        var existingDataset = await context.Datasets
            .Include(d => d.TestCases)
            .FirstOrDefaultAsync(d => d.DatasetId == entity.DatasetId);

        if (existingDataset == null)
            throw new KeyNotFoundException("Dataset not found");

        // 2. Cập nhật Dataset
        existingDataset.Name = entity.Name;
        existingDataset.Kind = entity.Kind;
        existingDataset.ProblemId = entity.ProblemId;

        // 3. Xử lý TestCases
        var existingTestCaseIds = existingDataset.TestCases.Select(tc => tc.TestCaseId).ToList();
        var newTestCases = entity.TestCases ?? new List<TestCase>();

        // 3a. Xóa TestCases không còn tồn tại
        var testCasesToRemove = existingDataset.TestCases
            .Where(existingTc => !newTestCases.Any(newTc => newTc.TestCaseId == existingTc.TestCaseId))
            .ToList();

        foreach (var testCaseToRemove in testCasesToRemove)
        {
            existingDataset.TestCases.Remove(testCaseToRemove);
        }

        // 3b. Thêm hoặc cập nhật TestCases
        foreach (var newTestCase in newTestCases)
        {

            var existingTestCase = existingDataset.TestCases
                .FirstOrDefault(tc => tc.TestCaseId == newTestCase.TestCaseId);

            if (existingTestCase == null)
            {
                if (newTestCase.TestCaseId == Guid.Empty)
                {
                    newTestCase.TestCaseId = Guid.NewGuid();
                }

                newTestCase.DatasetId = existingDataset.DatasetId;
                existingDataset.TestCases.Add(newTestCase);

                await context.TestCases.AddAsync(newTestCase);
            }
            else
            {
                // Cập nhật TestCase hiện tại
                existingTestCase.InputRef = newTestCase.InputRef;
                existingTestCase.OutputRef = newTestCase.OutputRef;
                existingTestCase.IndexNo = newTestCase.IndexNo;
                existingTestCase.Score = newTestCase.Score;

            }
        }

        await context.SaveChangesAsync();

        return existingDataset;
    }

    
    public async Task<bool> CheckDatasetExistsByUserIdAsync(Guid userId)
    {
        return await context.Datasets
            .Join(context.Problems,
                  dataset => dataset.ProblemId,
                  problem => problem.ProblemId,
                  (dataset, problem) => new { Dataset = dataset, Problem = problem })
            .AnyAsync(joined => joined.Problem.OwnerId == userId);
    }

}