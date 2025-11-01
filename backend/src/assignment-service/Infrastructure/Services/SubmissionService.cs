using AssignmentService.Application.Interfaces;
using AssignmentService.Domain.Entities;
using AssignmentService.Domain.Enums;

namespace AssignmentService.Application.Services;

public class SubmissionAppService : ISubmissionAppService
{
    private readonly ISubmissionRepository _repository;
    // private readonly IExecuteService _exec;
    public SubmissionAppService(ISubmissionRepository repository)
    {
        _repository = repository;
        // _exec = exec;
    }

    public Task<Submission> GetSubmission(Guid submissionId)
        => _repository.GetSubmission(submissionId);

    public Task<List<Submission>> GetAllUserSubmission(Guid userId, int pageNumber, int pageSize)
    {
        return _repository.GetAllSubmissionByUser(userId, pageNumber, pageSize);
    }

    public Task<List<Submission>> GetAllSubmissionByProblemIdAndUserId(Guid problemId, Guid userId, int pageNumber, int pageSize)
        => _repository.GetAllSubmissionByProblemIdAndUserId(problemId, userId, pageNumber, pageSize);

    public Task<List<Submission>> GetBestUserSubmissionByProblemId(Guid assignmentId, Guid problemId, int pageNumber, int pageSize)
        => _repository.GetBestSubmissionByProblemId(assignmentId, problemId, pageNumber, pageSize);

    public async Task<Submission> SubmitCode(Submission submission)
    {
        Submission new_submission = null!;
        try
        {
            new_submission = await _repository.SubmitCode(submission);
            // await _exec.ExecuteCode(new_submission);

            return new_submission;
        }
        catch (Exception ex)
        {
            await _repository.DeleteSubmission(new_submission.SubmissionId);
            throw new Exception(ex.Message);
        }
    }
    
    public Task<Submission> RunCode(Submission submission)
    {
        throw new NotImplementedException();
    }
    public Task<bool> DeleteSubmission(Guid submissionId)
        => _repository.DeleteSubmission(submissionId);

    public Task<bool> DeleteSubmissionByProblemId(Guid problemId)
        => _repository.DeleteSubmissionByProblemId(problemId);

    public Task<bool> DeleteSubmissionByUserId(Guid userId)
        => _repository.DeleteSubmissionByUserId(userId);

    public Task<int> GetNumberOfSubmissionPerProblemId(Guid assignmentId, Guid problemId, Guid userId)
        => _repository.GetNumberOfSubmissionPerProblemId(assignmentId, problemId, userId);

    public Task<int> GetNumberOfSubmission(Guid userId)
        => _repository.GetNumberOfSubmission(userId);

    public async Task<bool> UpdateSubmission(Submission submission)
    {
        return await _repository.UpdateSubmission(submission);
    }

    public Task<bool> UpdateSubmissionStatus(Guid submissionId, SubmissionStatus status)
    {
        return _repository.UpdateSubmissionStatus(submissionId, status);
    }
}