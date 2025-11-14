using AssignmentService.Domain.Entities;

namespace AssignmentService.Application.Interfaces.Repositories;

public interface ISubmissionRepository
{
    public Task<List<Submission>> GetAllSubmissionByUser(Guid userId, int pageNumber, int pageSize);
    public Task<List<Submission>> GetAllSubmissionByProblemIdAndUserId(Guid problemId, Guid userId, int pageNumber, int pageSize);
    public Task<List<BestSubmission>> GetBestSubmissionByProblemId(Guid assignmentUserId, Guid problemId, int pageNumber, int pageSize);
    // public Task<BestSubmission> GetBestSubmissionByProblemIdAndUserId(Guid assignmentUserId, Guid problemId);
    public Task<Submission> GetSubmission(Guid submissionId);
    public Task<Submission> GetRunningSubmissionByUserAndProblem(Guid userId, Guid problemId);
    public Task<Submission> AddSubmission(Submission submission);
    public Task<bool> DeleteSubmission(Guid submissionId);
    // public Task<bool> DeleteSubmissionByProblemId(Guid submissionId);
    // public Task<bool> DeleteSubmissionByUserId(Guid submissionId);
    public Task<bool> UpdateSubmission(Submission submission);
    // public Task<bool> UpdateSubmissionStatus(Guid submissionId, SubmissionStatus status);
    public Task<int> GetNumberOfSubmissionPerProblemId(Guid assignmentId, Guid problemId, Guid userId);
    public Task<int> GetNumberOfSubmission(Guid userId);
    public Task<List<BestSubmission>> GetMyBestSubmissionByAssignment(Guid assignmentId, List<Guid> problemId, Guid userId);
    public Task<BestSubmission?> GetBestSubmission(Guid assignmentId, Guid problemId, Guid userId);
    // public Task Detach(Submission submission);
}