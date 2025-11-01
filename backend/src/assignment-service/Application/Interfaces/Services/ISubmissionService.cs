using AssignmentService.Domain.Entities;
using AssignmentService.Domain.Enums;

namespace AssignmentService.Application.Interfaces;

public interface ISubmissionAppService
{
    Task<int> GetNumberOfSubmissionPerProblemId(Guid assignmentId, Guid problemId, Guid userId);
    Task<int> GetNumberOfSubmission(Guid userId);
    Task<Submission> GetSubmission(Guid submissionId);
    Task<List<Submission>> GetAllUserSubmission(Guid userId, int pageNumber = 1, int pageSize = 10);
    Task<List<Submission>> GetAllSubmissionByProblemIdAndUserId(Guid problemId, Guid userId, int pageNumber, int pageSize);
    Task<List<Submission>> GetBestUserSubmissionByProblemId(Guid assignmentId, Guid problemId, int pageNumber, int pageSize);
    Task<Submission> SubmitCode(Submission submission);
    Task<Submission> RunCode(Submission submission);
    Task<bool> UpdateSubmission(Submission submission);
    Task<bool> UpdateSubmissionStatus(Guid submissionId, SubmissionStatus status);
    Task<bool> DeleteSubmission(Guid submissionId);
    Task<bool> DeleteSubmissionByProblemId(Guid problemId);
    Task<bool> DeleteSubmissionByUserId(Guid userId);
}