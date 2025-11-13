using AssignmentService.Application.Interfaces.Repositories;
using AssignmentService.Application.Interfaces.Services;
using AssignmentService.Application.Interfaces.MessageBrokers;
using AssignmentService.Domain.Entities;
using AssignmentService.Domain.Enums;

namespace AssignmentService.Infrastructure.Services;

public class SubmissionService : ISubmissionService
{
    private readonly ISubmissionRepository _repository;
    private readonly IDatasetService _datasetService;
    private readonly IAssignmentService _assignmentService;
    private readonly IExecuteService _exec;
    public SubmissionService(ISubmissionRepository repository, IDatasetService datasetService, IAssignmentService assignmentService, IExecuteService exec)
    {
        _repository = repository;
        _datasetService = datasetService;
        _assignmentService = assignmentService;
        _exec = exec;
    }

    public async Task<Submission> GetSubmission(Guid submissionId)
    {
        try
        {
            var submission = await _repository.GetSubmission(submissionId);
            var dataset = await _datasetService.GetDatasetByIdAsync(submission.DatasetId);
            if (dataset?.Kind == DatasetKind.SAMPLE && (submission.Status == SubmissionStatus.Passed || submission.Status == SubmissionStatus.Failed))
            {
                await _repository.DeleteSubmission(submissionId);
            }
            return submission;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    public Task<List<Submission>> GetAllUserSubmission(Guid userId, int pageNumber, int pageSize)
    {
        return _repository.GetAllSubmissionByUser(userId, pageNumber, pageSize);
    }

    public Task<List<Submission>> GetAllSubmissionProblem(Guid problemId, Guid userId, int pageNumber, int pageSize)
        => _repository.GetAllSubmissionByProblemIdAndUserId(problemId, userId, pageNumber, pageSize);

    public Task<List<BestSubmission>> GetBestSubmissionByProblemId(Guid assignmentId, Guid problemId, int pageNumber, int pageSize)
        => _repository.GetBestSubmissionByProblemId(assignmentId, problemId, pageNumber, pageSize);

    public async Task<Submission> SubmitCode(Submission submission)
    {
        Submission new_submission = null!;
        try
        {
            var running_submission = await _repository.GetRunningSubmissionByUserAndProblem(submission.UserId, submission.ProblemId);
            if (running_submission.Status == SubmissionStatus.Running)
            {
                Console.WriteLine($"[x] User {submission.UserId} already has a running submission for problem {submission.ProblemId}");
                return running_submission;
            }
            
            submission.SubmissionId = Guid.NewGuid();
            submission.SubmittedAt = DateTime.Now;

            var assignment = await _assignmentService.GetAssignmentByIdAsync(submission.AssignmentId ?? Guid.Empty);
            if (assignment != null)
            {
                if (submission.SubmittedAt > assignment.EndTime)
                {
                    submission.isSubmitLate = true;
                }
                else
                {
                    submission.isSubmitLate = false;
                }

                if (!assignment.AllowLateSubmission)
                {
                    submission.Status = SubmissionStatus.Failed;
                    submission.ErrorMessage = "Late submissions are not allowed for this assignment.";
                }
            }

            var datasets = await _datasetService.GetDatasetsByProblemIdAsync(submission.ProblemId, DatasetKind.PRIVATE | DatasetKind.PUBLIC | DatasetKind.OFFICIAL);
            if (datasets == null || datasets.Count == 0)
            {
                Console.WriteLine($"[x] No dataset found for problem {submission.ProblemId}");
                return new Submission();
            }

            submission.DatasetId = datasets.FirstOrDefault()?.DatasetId ?? Guid.Empty;

            if (submission.DatasetId == Guid.Empty)
            {
                Console.WriteLine($"[x] No dataset found for problem {submission.ProblemId}");
                return new Submission();
            }

            new_submission = await _repository.AddSubmission(submission);
            if (new_submission.Status == SubmissionStatus.Pending)
            {
                await _exec.ExecuteCode(new_submission);
            }
            Console.WriteLine($"Waiting for Judge submission");

            return new_submission;
        }
        catch (Exception ex)
        {
            await _repository.DeleteSubmission(new_submission.SubmissionId);
            throw new Exception(ex.Message);
        }
    }
    public async Task<Submission> RunCode(Submission submission)
    {
        try
        {
            var running_submission = await _repository.GetRunningSubmissionByUserAndProblem(submission.UserId, submission.ProblemId);
            if (running_submission.Status == SubmissionStatus.Running)
            {
                Console.WriteLine($"[x] User {submission.UserId} already has a running submission for problem {submission.ProblemId}");
                return running_submission;
            }

            submission.SubmissionId = Guid.NewGuid();
            submission.SubmittedAt = DateTime.Now;
            var datasets = await _datasetService.GetDatasetsByProblemIdAsync(submission.ProblemId, DatasetKind.SAMPLE);
            if (datasets == null || datasets.Count == 0)
            {
                Console.WriteLine($"[x] No dataset found for problem {submission.ProblemId}");
                return new Submission();
            }

            submission.DatasetId = datasets.FirstOrDefault(dt => dt.Kind == DatasetKind.SAMPLE && dt.ProblemId == submission.ProblemId)?.DatasetId ?? Guid.Empty;

            if (submission.DatasetId == Guid.Empty)
            {
                Console.WriteLine($"[x] No sample dataset found for problem {submission.ProblemId}");
                return new Submission();
            }

            var new_submission = await _repository.AddSubmission(submission);
            await _exec.ExecuteCode(new_submission);
            Console.WriteLine($"Waiting for Judge run code");

            return new_submission;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
    // public Task<bool> DeleteSubmission(Guid submissionId)
    //     => _repository.DeleteSubmission(submissionId);

    // public Task<bool> DeleteSubmissionByProblemId(Guid problemId)
    //     => _repository.DeleteSubmissionByProblemId(problemId);

    // public Task<bool> DeleteSubmissionByUserId(Guid userId)
    //     => _repository.DeleteSubmissionByUserId(userId);

    public Task<int> GetNumberOfSubmissionPerProblemId(Guid assignmentId, Guid problemId, Guid userId)
        => _repository.GetNumberOfSubmissionPerProblemId(assignmentId, problemId, userId);

    public Task<int> GetNumberOfSubmission(Guid userId)
        => _repository.GetNumberOfSubmission(userId);

    public async Task<bool> UpdateSubmission(Submission submission)
    {
        try
        {
            var BestSubmission = await _repository.GetBestSubmission(submission.AssignmentId ?? Guid.Empty, submission.ProblemId, submission.UserId);
            if (BestSubmission == null || submission.Score > BestSubmission.Score)
            {
                var score = submission.Score - (BestSubmission?.Score ?? 0);
                await _assignmentService.UpdateAssignmentUserScoreAsync(submission.AssignmentId ?? Guid.Empty, submission.UserId, score);
                Console.WriteLine($"[x] Added/Updated best submission for user {submission.UserId} on problem {submission.ProblemId}");
            }

            return await _repository.UpdateSubmission(submission);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    public async Task<List<BestSubmission>> GetMyBestSubmissionByAssignment(Guid assignmentId, List<Guid> problemId, Guid userId)
    {
        return await _repository.GetMyBestSubmissionByAssignment(assignmentId, problemId, userId);
    }

    public async Task<int> Getscore(Submission submission)
    {
        try
        {
            int score = 0;
            if (submission.TotalTestcase == 0) return score;
            Guid assignmentId = submission.AssignmentId ?? Guid.Empty;
            var assignment = await _assignmentService.GetAssignmentProblemAsync(assignmentId, submission.ProblemId);
            if (assignment == null) return score;

            score = (int)Math.Round((double)(submission.PassedTestcase * assignment.Points) / submission.TotalTestcase);
            return score;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    // public Task<bool> UpdateSubmissionStatus(Guid submissionId, SubmissionStatus status)
    // {
    //     return _repository.UpdateSubmissionStatus(submissionId, status);
    // }
    public async Task<BestSubmission?> GetBestSubmission(Guid assignmentId, Guid problemId, Guid userId)
    {
        return await _repository.GetBestSubmission(assignmentId, problemId, userId);
    }
}