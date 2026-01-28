using Microsoft.EntityFrameworkCore;
using AssignmentService.Domain.Entities;
using AssignmentService.Domain.Enums;
using AssignmentService.Application.Interfaces.Repositories;
using AssignmentService.Infrastructure.EF;

namespace AssignmentService.Infrastructure.Repositories;

public class SubmissionRepository : ISubmissionRepository
{
    private readonly AssignmentDbContext _context;

    public SubmissionRepository(AssignmentDbContext context)
    {
        _context = context;
    }

    public async Task<Submission> AddSubmission(Submission submission)
    {
        try
        {
            _context.Submissions.Add(submission);
            if (await _context.SaveChangesAsync() > 0)
            {
                Console.WriteLine($"[x] Added submission {submission.SubmissionId} to database");
                return submission;
            }
            Console.WriteLine($"[x] Failed to add submission to database");
            return new Submission();
        }
        catch (Exception ex)
        {
            var innerMessage = ex.InnerException?.Message ?? "No inner exception";
            Console.WriteLine($"[AddSubmission Error] {ex.Message}");
            Console.WriteLine($"[AddSubmission Inner Error] {innerMessage}");
            throw new Exception($"{ex.Message} Inner: {innerMessage}", ex);
        }
    }

    public async Task<bool> DeleteSubmission(Guid submissionId)
    {
        // First, try to find if the entity is already being tracked
        var trackedEntity = _context.ChangeTracker.Entries<Submission>()
            .FirstOrDefault(e => e.Entity.SubmissionId == submissionId);

        if (trackedEntity != null)
        {
            // If it's already tracked, just remove it directly
            _context.Submissions.Remove(trackedEntity.Entity);
            Console.WriteLine($"[x] Deleted tracked submission {submissionId} from database");
            return await _context.SaveChangesAsync() > 0;
        }

        // If not tracked, fetch without tracking and remove
        var submission = await _context.Submissions.AsNoTracking().FirstOrDefaultAsync(p => p.SubmissionId == submissionId);
        if (submission != null)
        {
            _context.Submissions.Remove(submission);
            Console.WriteLine($"[x] Deleted submission {submissionId} from database");
            return await _context.SaveChangesAsync() > 0;
        }
        return false;
    }

    public async Task<bool> DeleteSubmissionByProblemId(Guid problemId)
    {
        var submission = await _context.Submissions.AsNoTracking().FirstOrDefaultAsync(p => p.ProblemId == problemId);
        while (submission != null)
        {
            _context.Submissions.Remove(submission);
            submission = await _context.Submissions.AsNoTracking().FirstOrDefaultAsync(p => p.ProblemId == problemId);
        }
        Console.WriteLine($"[x] Deleted all submissions for problem {problemId} from database");
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteSubmissionByUserId(Guid userId)
    {
        var submission = await _context.Submissions.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == userId);
        while (submission != null)
        {
            _context.Submissions.Remove(submission);
            submission = await _context.Submissions.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == userId);
        }
        Console.WriteLine($"[x] Deleted all submissions for user {userId} from database");
        return await _context.SaveChangesAsync() > 0;
    }

    public  Task Detach(Submission submission)
    {
        _context.Entry(submission).State = EntityState.Detached;
        return Task.CompletedTask;
    }

    public async Task<List<Submission>> GetAllSubmissionByProblemIdAndUserId(Guid problemId, Guid userId, int pageNumber, int pageSize)
    {
        var submissions = await _context.Submissions
            .AsNoTracking()
            .Where(p => p.ProblemId == problemId && p.UserId == userId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        if (submissions == null)
        {
            Console.WriteLine($"[x] No submissions found for problem {problemId} and user {userId}");
            return new List<Submission>();
        }
        Console.WriteLine($"[x] Retrieved {submissions.Count} submissions for problem {problemId} and user {userId}");
        return submissions;
    }

    public async Task<List<Submission>> GetAllSubmissionByUser(Guid userId, int pageNumber, int pageSize)
    {
        Console.WriteLine($"[x] Retrieving submissions for user {userId}, page {pageNumber}, size {pageSize}");
        return await _context.Submissions
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.SubmittedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<BestSubmission>> GetBestSubmissionByProblemId(Guid assignmentId, Guid problemId, int pageNumber, int pageSize)
    {
        Console.WriteLine($"[x] Retrieving best submissions for problem {problemId}, page {pageNumber}, size {pageSize}");
        return await _context.BestSubmissions
            .AsNoTracking()
            .Where(s => s.ProblemId == problemId && s.AssignmentId == assignmentId)
            .OrderByDescending(s => s.Score)
            .ThenBy(s => s.TotalTime)
            .ThenBy(s => s.TotalMemory)
            .ThenByDescending(s => s.SubmittedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<BestSubmission>> GetMyBestSubmissionByAssignment(Guid assignmentId, List<Guid> problemId, Guid userId)
    {
        return await _context.BestSubmissions
            .AsNoTracking()
            .Where(s => s.AssignmentId == assignmentId && problemId.Contains(s.ProblemId) && s.UserId == userId)
            .OrderByDescending(s => s.Score)
            .ThenBy(s => s.TotalTime)
            .ThenBy(s => s.TotalMemory)
            .ThenByDescending(s => s.SubmittedAt)
            .ToListAsync();
    }

    public async Task<BestSubmission?> GetBestSubmission(Guid assignmentId, Guid problemId, Guid userId)
    {
        return await _context.BestSubmissions
            .AsNoTracking()
            .Where(s => s.AssignmentId == assignmentId && s.ProblemId == problemId && s.UserId == userId)
            .OrderByDescending(s => s.Score)
            .ThenBy(s => s.TotalTime)
            .ThenBy(s => s.TotalMemory)
            .ThenByDescending(s => s.SubmittedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<int> GetNumberOfSubmission(Guid userId)
    {
        return await _context.Submissions
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .CountAsync();
    }

    public async Task<int> GetNumberOfSubmissionPerProblemId(Guid assignmentId, Guid problemId, Guid userId)
    {
        return await _context.Submissions
            .AsNoTracking()
            .Where(s => s.AssignmentId == assignmentId && s.ProblemId == problemId && s.UserId == userId)
            .CountAsync();
    }

    public async Task<int> GetTotalSubmissionCountPerProblemIdAndAssignment(Guid assignmentId, Guid problemId)
    {
        return await _context.Submissions
            .AsNoTracking()
            .Where(s => s.AssignmentId == assignmentId && s.ProblemId == problemId)
            .CountAsync();
    }

    public async Task<Submission> GetSubmission(Guid submissionId)
    {
        var submission = await _context.Submissions.AsNoTracking().FirstOrDefaultAsync(s => s.SubmissionId == submissionId);
        if (submission != null)
        {
            Console.WriteLine($"[x] Retrieved submission {submissionId} from database");
            return submission;
        }
        Console.WriteLine($"[x] Submission {submissionId} not found in database");
        return new Submission();
    }

    public async Task<Submission> GetRunningSubmissionByUserAndProblem(Guid userId, Guid problemId)
    {
        var submission = await _context.Submissions
            .AsNoTracking()
            .Where(s => s.UserId == userId && s.ProblemId == problemId && s.Status == SubmissionStatus.Running)
            .FirstOrDefaultAsync();
        if (submission != null)
        {
            Console.WriteLine($"[x] Retrieved running submission for user {userId} and problem {problemId} from database");
            return submission;
        }
        Console.WriteLine($"[x] No running submission found for user {userId} and problem {problemId} in database");
        return new Submission();
    }

    public async Task<bool> UpdateSubmission(Submission submission)
    {
        // var _submisison = await _context.Submissions.AsNoTracking().FirstOrDefaultAsync(s => s.SubmissionId == submission.SubmissionId);
        // if (_submisison != null)
        // {

        // }
        try
        {
            _context.Submissions.Update(submission);
            Console.WriteLine($"[x] Updated submission {submission.SubmissionId} in database");
            return await _context.SaveChangesAsync() > 0;
        }
        catch (System.Exception ex)
        {
            throw new Exception($"Failed to update submission {submission.SubmissionId} in database: ", ex);
        }
        // Console.WriteLine($"[x] Submission {submission.SubmissionId} not found in database");
        // return false;
    }

    public async Task<List<Submission>> GetAllSubmissionByAssignmentAndProblem(Guid assignmentId, Guid problemId, int pageNumber, int pageSize)
    {
        try
        {
            var submissions =  await _context.Submissions
                .AsNoTracking()
                .Where(p => p.AssignmentId == assignmentId && p.ProblemId == problemId)
                .OrderByDescending(p => p.SubmittedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return submissions;
        }
        catch (System.Exception ex)
        {
            throw new Exception(ex.Message, ex);
        }
    }

    public async Task<AssignmentService.Application.DTOs.Responses.StatsPerProblemResponse> GetStatsPerProblem(Guid assignmentId, Guid problemId)
    {
        var total = await _context.Submissions
            .AsNoTracking()
            .Where(s => s.AssignmentId == assignmentId && s.ProblemId == problemId)
            .CountAsync();

        var passed = await _context.Submissions
            .AsNoTracking()
            .Where(s => s.AssignmentId == assignmentId && s.ProblemId == problemId && s.PassedTestcase == s.TotalTestcase)
            .CountAsync();

        var failed = await _context.Submissions
            .AsNoTracking()
            .Where(s => s.AssignmentId == assignmentId && s.ProblemId == problemId && s.PassedTestcase == 0)
            .CountAsync();

        var partial = await _context.Submissions
            .AsNoTracking()
            .Where(s => s.AssignmentId == assignmentId && s.ProblemId == problemId && s.PassedTestcase > 0 && s.PassedTestcase < s.TotalTestcase)
            .CountAsync();

        return new AssignmentService.Application.DTOs.Responses.StatsPerProblemResponse
        {
            Total = total,
            Passed = passed,
            Failed = failed,
            Partial = partial
        };
    }

    // public async Task<bool> UpdateSubmissionStatus(Guid submissionId, SubmissionStatus status)
    // {
    //     var submission = await _context.Submissions.AsNoTracking().FirstOrDefaultAsync(s => s.SubmissionId == submissionId);
    //     if (submission != null)
    //     {
    //         submission.Status = status;
    //         _context.Submissions.Update(submission);
    //         Console.WriteLine($"[x] Updated submission {submissionId} status to {status}");
    //         return await _context.SaveChangesAsync() > 0;
    //     }
    //     Console.WriteLine($"[x] Submission {submissionId} not found in database");
    //     return false;
    //     public async Task<SubmissionStatsResponse> GetSubmissionStatsPerProblemIdAndAssignment(Guid assignmentId, Guid problemId)

    public async Task<int> GetTotalSubmissionsCountAsync()
    {
        return await _context.Submissions.CountAsync();
    }
}
