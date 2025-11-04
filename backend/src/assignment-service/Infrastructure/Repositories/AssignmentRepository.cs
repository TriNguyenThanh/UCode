using Microsoft.EntityFrameworkCore;
using AssignmentService.Application.Interfaces.Repositories;
using AssignmentService.Domain.Entities;
using AssignmentService.Infrastructure.EF;
using System.Linq.Expressions;

namespace AssignmentService.Infrastructure.Repositories;

public class AssignmentRepository : IAssignmentRepository
{
    private readonly AssignmentDbContext _context;

    public AssignmentRepository(AssignmentDbContext context)
    {
        _context = context;
    }

    public async Task<Assignment> AddAsync(Assignment entity)
    {
        await _context.Assignments.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task AddRangeAsync(IEnumerable<Assignment> entities)
    {
        await _context.Assignments.AddRangeAsync(entities);
        await _context.SaveChangesAsync();
    }

    public async Task<Assignment?> GetByIdAsync(Guid id)
    {
        return await _context.Assignments
            .AsNoTracking()
            .Include(a => a.AssignmentProblems)
                .ThenInclude(ap => ap.Problem)
            .FirstOrDefaultAsync(a => a.AssignmentId == id);
    }

    public async Task<Assignment?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _context.Assignments
            .AsNoTracking()
            .Include(a => a.AssignmentProblems)
                .ThenInclude(ap => ap.Problem)
            .FirstOrDefaultAsync(a => a.AssignmentId == id);
    }

    /// <summary>
    /// Lấy assignment với chỉ ProblemId và Title của problems (tối ưu cho hiển thị danh sách)
    /// </summary>
    public async Task<Assignment?> GetByIdWithProblemBasicsAsync(Guid id)
    {
        var assignment = await _context.Assignments
            .AsNoTracking()
            .Where(a => a.AssignmentId == id)
            .Select(a => new
            {
                Assignment = a,
                Problems = a.AssignmentProblems
                    .OrderBy(ap => ap.OrderIndex)
                    .Select(ap => new
                    {
                        ap.AssignmentId,
                        ap.ProblemId,
                        ap.Points,
                        ap.OrderIndex,
                        ProblemTitle = ap.Problem.Title,
                        ProblemCode = ap.Problem.Code,
                        ProblemDifficulty = ap.Problem.Difficulty
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (assignment == null) return null;

        // Map lại vào Assignment entity
        var result = assignment.Assignment;
        result.AssignmentProblems = assignment.Problems.Select(p => new AssignmentProblem
        {
            AssignmentId = p.AssignmentId,
            ProblemId = p.ProblemId,
            Points = p.Points,
            OrderIndex = p.OrderIndex,
            Problem = new Problem
            {
                ProblemId = p.ProblemId,
                Title = p.ProblemTitle,
                Code = p.ProblemCode,
                Difficulty = p.ProblemDifficulty
            }
        }).ToList();

        return result;
    }

    public async Task<List<Assignment>> GetByTeacherIdAsync(Guid teacherId)
    {
        return await _context.Assignments
            .AsNoTracking()
            .Include(a => a.AssignmentProblems)
            .Where(a => a.AssignedBy == teacherId)
            .OrderByDescending(a => a.AssignedAt)
            .ToListAsync();
    }

    public async Task<List<Assignment>> GetByClassIdAsync(Guid classId)
    {
        return await _context.Assignments
               .AsNoTracking()
               .Include(a => a.AssignmentProblems)
               .Where(a => a.ClassId == classId)
               .OrderByDescending(a => a.AssignedAt)
               .ToListAsync();
    }

    public async Task<List<Assignment>> GetByUserIdAsync(Guid UserId)
    {
        return await _context.Assignments
            .AsNoTracking()
            .Include(a => a.AssignmentUsers.Where(d => d.UserId == UserId))
            .Where(a => a.AssignmentUsers.Any(d => d.UserId == UserId))
            .OrderByDescending(a => a.AssignedAt)
            .ToListAsync();
    }

    public async Task<Assignment?> GetWithStatisticsAsync(Guid id)
    {
        return await _context.Assignments
            .AsNoTracking()
            .Include(a => a.AssignmentUsers)
            .FirstOrDefaultAsync(a => a.AssignmentId == id);
    }


    public async Task<List<Assignment>> FindAsync(Expression<Func<Assignment, bool>> predicate)
    {
        return await _context.Assignments
            .AsNoTracking()
            .Where(predicate)
            .ToListAsync();
    }

    public async Task<Assignment> UpdateAsync(Assignment entity)
    {
        var existingAssignment = await _context.Assignments
            .Include(a => a.AssignmentProblems)
            .FirstOrDefaultAsync(a => a.AssignmentId == entity.AssignmentId);

        if (existingAssignment == null)
            throw new KeyNotFoundException("Assignment not found");

        existingAssignment.AssignmentType = entity.AssignmentType;
        existingAssignment.ClassId = entity.ClassId;
        existingAssignment.Title = entity.Title;
        existingAssignment.Description = entity.Description;
        existingAssignment.StartTime = entity.StartTime;
        existingAssignment.EndTime = entity.EndTime;
        // existingAssignment.AssignedAt = entity.AssignedAt;
        // existingAssignment.TotalPoints = entity.TotalPoints;
        existingAssignment.AllowLateSubmission = entity.AllowLateSubmission;
        existingAssignment.Status = entity.Status;

        var newProblems = (entity.AssignmentProblems ?? new List<AssignmentProblem>()).ToList();

        existingAssignment.TotalPoints = newProblems.Sum(p => p.Points);
        existingAssignment.AssignmentProblems = newProblems;

        // _context.Assignments.Update(existingAssignment);

        await _context.SaveChangesAsync();

        var newMaxScore = await GetAssignmentMaxScoreAsync(entity.AssignmentId);
        await UpdateAssignmentUsersMaxScoreAsync(entity.AssignmentId, newMaxScore);

        return await GetByIdWithProblemBasicsAsync(entity.AssignmentId) ?? existingAssignment;
    }

    public async Task<bool> RemoveAsync(Guid id)
    {
        var entity = await _context.Assignments.FindAsync(id);
        if (entity == null)
            return false;
        _context.Assignments.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveRangeAsync(IEnumerable<Guid> ids)
    {
        var assignments = await _context.Assignments
            .Where(a => ids.Contains(a.AssignmentId))
            .ToListAsync();

        _context.Assignments.RemoveRange(assignments);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Assignment>> GetAllAsync()
    {
        return await _context.Assignments.AsNoTracking().ToListAsync();
    }

    public async Task<(List<Assignment> Items, int Total)> GetPagedAsync(int page, int pageSize, Expression<Func<Assignment, bool>>? predicate = null, Func<IQueryable<Assignment>, IOrderedQueryable<Assignment>>? orderBy = null)
    {
        var query = _context.Assignments.AsQueryable();

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

    //==================== AssignmentUser methods =====================
    public async Task<AssignmentUser> AddAssignmentUserAsync(AssignmentUser detail)
    {
        await _context.AssignmentUsers.AddAsync(detail);
        await _context.SaveChangesAsync();
        return detail;
    }

    public async Task<List<AssignmentUser>> AddAssignmentUsersAsync(List<AssignmentUser> details)
    {
        await _context.AssignmentUsers.AddRangeAsync(details);
        await _context.SaveChangesAsync();
        return details;
    }

    public async Task<AssignmentUser?> GetAssignmentUserAsync(Guid assignmentId, Guid UserId)
    {
        return await _context.AssignmentUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.AssignmentId == assignmentId && d.UserId == UserId);
    }

    public async Task<AssignmentUser?> GetAssignmentUserByIdAsync(Guid AssignmentUserId)
    {
        return await _context.AssignmentUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.AssignmentUserId == AssignmentUserId);
    }

    public async Task<List<AssignmentUser>> GetAssignmentUsersByAssignmentAsync(Guid assignmentId)
    {
        return await _context.AssignmentUsers
            .AsNoTracking()
            .Where(d => d.AssignmentId == assignmentId)
            .OrderBy(d => d.AssignedAt)
            .ToListAsync();
    }

    public async Task<AssignmentUser> UpdateAssignmentUserAsync(AssignmentUser detail)
    {
        _context.AssignmentUsers.Update(detail);
        await _context.SaveChangesAsync();
        return detail;
    }

    public async Task RemoveAssignmentUsersByAssignmentAsync(Guid assignmentId)
    {
        var details = await _context.AssignmentUsers
            .Where(d => d.AssignmentId == assignmentId)
            .ToListAsync();
        if (details.Count == 0) return;
        _context.AssignmentUsers.RemoveRange(details);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetAssignmentMaxScoreAsync(Guid assignmentId)
    {
        var sum = await _context.AssignmentProblems
            .Where(ap => ap.AssignmentId == assignmentId)
            .SumAsync(ap => (int?)ap.Points) ?? 0;
        return sum;
    }

    public async Task UpdateAssignmentUsersMaxScoreAsync(Guid assignmentId, int maxScore)
    {
        var details = await _context.AssignmentUsers
            .Where(d => d.AssignmentId == assignmentId)
            .ToListAsync();
        if (details.Count == 0) return;
        foreach (var d in details)
        {
            d.MaxScore = maxScore;
        }
        await _context.SaveChangesAsync();
    }

    // ================[ AssignmentProblem methods ]==================
    public async Task<List<AssignmentProblem>> AddAssignmentProblemsAsync(List<AssignmentProblem> assignmentProblems)
    {
        // Tạo entities mới để tránh tracking conflict
        var newEntities = assignmentProblems.Select(ap => new AssignmentProblem
        {
            AssignmentId = ap.AssignmentId,
            ProblemId = ap.ProblemId,
            Points = ap.Points,
            OrderIndex = ap.OrderIndex
        }).ToList();

        await _context.AssignmentProblems.AddRangeAsync(newEntities);
        await _context.SaveChangesAsync();
        return newEntities;
    }

    public async Task<AssignmentProblem?> GetAssignmentProblemAsync(Guid assignmentId, Guid problemId)
    {
        return await _context.AssignmentProblems
            .AsNoTracking()
            .FirstOrDefaultAsync(ap => ap.AssignmentId == assignmentId && ap.ProblemId == problemId);
    }

    // ================[ BestSubmission methods ]==================
    public async Task<BestSubmission> AddSubmissionAsync(BestSubmission submission)
    {
        await _context.BestSubmissions.AddAsync(submission);
        await _context.SaveChangesAsync();
        return submission;
    }

    public async Task<List<BestSubmission>> AddSubmissionsAsync(List<BestSubmission> submissions)
    {
        await _context.BestSubmissions.AddRangeAsync(submissions);
        await _context.SaveChangesAsync();
        return submissions;
    }

    public async Task<BestSubmission?> GetSubmissionAsync(Guid AssignmentUserId, Guid problemId)
    {
        return await _context.BestSubmissions
            .FirstOrDefaultAsync(s => s.AssignmentUserId == AssignmentUserId && s.ProblemId == problemId);
    }

    public async Task<BestSubmission?> GetSubmissionByIdAsync(Guid submissionId)
    {
        return await _context.BestSubmissions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);
    }

    public Task<List<BestSubmission>> GetSubmissionsByAssignmentUserAsync(Guid AssignmentUserId)
    {
        // return await _context.Submissions
        //     .AsNoTracking()
        //     .Where(s => s.AssignmentUserId == AssignmentUserId)
        //     .OrderBy(s => s.AssignmentUser.Assignment.AssignmentProblems
        //         .FirstOrDefault(ap => ap.ProblemId == s.ProblemId)!.OrderIndex)
        //     .ToListAsync();
        throw new NotImplementedException("Cái này thuộc bên Trí");
    }

    public Task<List<BestSubmission>> GetSubmissionsByAssignmentAsync(Guid assignmentId)
    {
        // return await _context.Submissions
        //     .Where(s => s.AssignmentUser.AssignmentId == assignmentId)
        //     .OrderBy(s => s.AssignmentUser.UserId)
        //     .ThenBy(s => s.AssignmentUser.Assignment.AssignmentProblems
        //         .FirstOrDefault(ap => ap.ProblemId == s.ProblemId)!.OrderIndex)
        //     .ToListAsync();
        throw new NotImplementedException("Cái này thuộc bên Trí");
    }

    public async Task<BestSubmission> UpdateSubmissionAsync(BestSubmission submission)
    {
        _context.BestSubmissions.Update(submission);
        await _context.SaveChangesAsync();
        return submission;
    }

    public async Task<bool> DeleteSubmissionAsync(Guid submissionId)
    {
        var submission = await _context.BestSubmissions.FindAsync(submissionId);
        if (submission == null)
            return false;

        _context.BestSubmissions.Remove(submission);
        await _context.SaveChangesAsync();
        return true;
    }


    public async Task<int> CountAsync(Expression<Func<Assignment, bool>>? predicate = null)
    {
        IQueryable<Assignment> query = _context.Assignments;

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        return await query.CountAsync();

    }

    public async Task<bool> AnyAsync(Expression<Func<Assignment, bool>> predicate)
    {
        return await _context.Assignments.AnyAsync(predicate);
    }

    public async Task<Guid?> GetAssignmentOwnerIdAsync(Guid assignmentId)
    {
        return await _context.Assignments
            .Where(a => a.AssignmentId == assignmentId)
            .Select(a => (Guid?)a.AssignedBy)
            .FirstOrDefaultAsync();
    }

    public async Task<(bool exists, Guid? ownerId)> CheckAssignmentExistsAndGetOwnerAsync(Guid assignmentId)
    {
        var result = await _context.Assignments
            .Where(a => a.AssignmentId == assignmentId)
            .Select(a => new { a.AssignedBy })
            .FirstOrDefaultAsync();

        return result != null ? (true, result.AssignedBy) : (false, null);
    }
}
