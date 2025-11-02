using Microsoft.EntityFrameworkCore;
using AssignmentService.Domain.Entities;
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

    public async Task<Submission> SubmitCode(Submission submission)
    {
        if (string.IsNullOrEmpty(submission.SubmissionId.ToString()))
        {
            submission.SubmissionId = Guid.NewGuid();
            submission.SubmittedAt = DateTime.Now;
        }
        else
        {
            return new Submission();
        }
        _context.Submissions.Add(submission);
        if (await _context.SaveChangesAsync() > 0)
        {
            Console.WriteLine($"[x] Added submission {submission.SubmissionId} to database");
            return submission;
        }
        Console.WriteLine($"[x] Failed to add submission to database");
        return new Submission();
    }

    public async Task<bool> DeleteSubmission(Guid submissionId)
    {
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

    public Task Detach(Submission submission)
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
            .Where(s => s.ProblemId == problemId && s.AssignmentUserId == assignmentId)
            .OrderByDescending(s => s.Score)
            .ThenBy(s => s.UpdatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public Task<int> GetNumberOfSubmission(Guid userId)
    {
        return _context.Submissions
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .CountAsync();
    }

    public Task<int> GetNumberOfSubmissionPerProblemId(Guid assignmentId, Guid problemId, Guid userId)
    {
        return _context.Submissions
            .AsNoTracking()
            .Where(s => s.AssignmentUserId == assignmentId && s.ProblemId == problemId && s.UserId == userId)
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

    public async Task<bool> UpdateSubmission(Submission submission)
    {
        var _submisison = await _context.Submissions.AsNoTracking().FirstOrDefaultAsync(s => s.SubmissionId == submission.SubmissionId);
        if (_submisison != null)
        {
            _context.Submissions.Update(submission);
            Console.WriteLine($"[x] Updated submission {submission.SubmissionId} in database");
            return await _context.SaveChangesAsync() > 0;
        }
        Console.WriteLine($"[x] Submission {submission.SubmissionId} not found in database");
        return false;
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
    // }
}