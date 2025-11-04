using AssignmentService.Application.Interfaces.Repositories;
using AssignmentService.Application.Interfaces.Services;
using AssignmentService.Domain.Entities;
using AssignmentService.Domain.Enums;

namespace AssignmentService.Infrastructure.Services;

public class SubmissionService : ISubmissionService
{
    private readonly ISubmissionRepository _repository;
    private readonly IDatasetRepository _datasetRepository;
    // private readonly IExecuteService _exec;
    public SubmissionService(ISubmissionRepository repository, IDatasetRepository datasetRepository)
    {
        _repository = repository;
        _datasetRepository = datasetRepository;
        // _exec = exec;
    }

    public async Task<Submission> GetSubmission(Guid submissionId){
        try{
            var submission = await _repository.GetSubmission(submissionId);
            var dataset = await _datasetRepository.GetByIdAsync(submission.DatasetId);
            if (dataset?.Kind == DatasetKind.SAMPLE && (submission.Status == SubmissionStatus.Done || submission.Status == SubmissionStatus.Failed))
            {
                await _repository.DeleteSubmission(submissionId);
            }
            return submission; 
        }
        catch(Exception ex){
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
            submission.SubmissionId = Guid.NewGuid();
            submission.SubmittedAt = DateTime.Now;
            var datasets = await _datasetRepository.GetByProblemIdAsync(submission.ProblemId);
            if (datasets == null || datasets.Count == 0)
            {
                Console.WriteLine($"[x] No dataset found for problem {submission.ProblemId}");
                return new Submission();
            }
            else
            {
                var dataset = datasets.FirstOrDefault(ds => ds.Kind == DatasetKind.PRIVATE);
                if (dataset != null)
                {
                    submission.DatasetId = dataset.DatasetId;
                }
            }
            new_submission = await _repository.AddSubmission(submission);
            // await _exec.ExecuteCode(new_submission);
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
            submission.SubmissionId = Guid.NewGuid();
            submission.SubmittedAt = DateTime.Now;
            var datasets = await _datasetRepository.GetByProblemIdAsync(submission.ProblemId);
            if (datasets == null || datasets.Count == 0)
            {
                Console.WriteLine($"[x] No dataset found for problem {submission.ProblemId}");
                return new Submission();
            }
            else
            {
                var dataset = datasets.FirstOrDefault(ds => ds.Kind == DatasetKind.SAMPLE);
                if (dataset != null)
                {
                    submission.DatasetId = dataset.DatasetId;
                }
            }
            var new_submission = await _repository.AddSubmission(submission);
            // await _exec.ExecuteCode(new_submission);
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
        return await _repository.UpdateSubmission(submission);
    }

    // public Task<bool> UpdateSubmissionStatus(Guid submissionId, SubmissionStatus status)
    // {
    //     return _repository.UpdateSubmissionStatus(submissionId, status);
    // }
}