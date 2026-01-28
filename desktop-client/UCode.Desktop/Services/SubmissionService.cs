using System.Collections.Generic;
using System.Threading.Tasks;
using UCode.Desktop.Models;

namespace UCode.Desktop.Services
{
    public class SubmissionService
    {
        private readonly ApiService _apiService;

        public SubmissionService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<ApiResponse<List<BestSubmission>>> GetBestSubmissionsAsync(string assignmentId, string problemId, int pageNumber = 1, int pageSize = 10)
        {
            return await _apiService.GetAsync<List<BestSubmission>>(
                $"api/v1/submissions/assignment/{assignmentId}/problem/{problemId}/best?pageNumber={pageNumber}&pageSize={pageSize}"
            );
        }

        public async Task<ApiResponse<BestSubmission>> GetBestSubmissionForStudentAsync(string assignmentId, string problemId, string userId)
        {
            return await _apiService.GetAsync<BestSubmission>(
                $"api/v1/submissions/assignment/{assignmentId}/problem/{problemId}/student/{userId}/best"
            );
        }

        public async Task<ApiResponse<Submission>> GetSubmissionAsync(string submissionId)
        {
            return await _apiService.GetAsync<Submission>($"api/v1/submissions/{submissionId}");
        }

        public async Task<ApiResponse<Submission>> RunCodeAsync(RunCodeRequest request)
        {
            return await _apiService.PostAsync<Submission>("api/v1/submissions/run-code", request);
        }

        public async Task<ApiResponse<Submission>> SubmitCodeAsync(SubmitCodeRequest request)
        {
            return await _apiService.PostAsync<Submission>("api/v1/submissions/submit-code", request);
        }

        public async Task<ApiResponse<List<Submission>>> GetSubmissionsByProblemAsync(string problemId, int pageNumber = 1, int pageSize = 10)
        {
            return await _apiService.GetAsync<List<Submission>>(
                $"api/v1/submissions/problem/{problemId}?pageNumber={pageNumber}&pageSize={pageSize}"
            );
        }

        public async Task<ApiResponse<PagedResultDto<Submission>>> GetSubmissionsByAssignmentAndProblemAsync(string assignmentId, string problemId, int pageNumber = 1, int pageSize = 10)
        {
            return await _apiService.GetAsync<PagedResultDto<Submission>>(
                $"api/v1/submissions/assignment/{assignmentId}/problem/{problemId}?pageNumber={pageNumber}&pageSize={pageSize}"
            );
        }

        public async Task<ApiResponse<int>> GetTotalSubmissionCountAsync(string assignmentId, string problemId)
        {
            return await _apiService.GetAsync<int>(
                $"api/v1/submissions/assignment/{assignmentId}/problem/{problemId}/total-count"
            );
        }

        public async Task<ApiResponse<StatsPerProblemResponse>> GetStatsPerProblemAsync(string assignmentId, string problemId)
        {
            return await _apiService.GetAsync<StatsPerProblemResponse>(
                $"api/v1/submissions/assignment/{assignmentId}/problem/{problemId}/stats"
            );
        }
        public async Task<ApiResponse<List<Submission>>> GetUserSubmissionsAsync(int pageNumber = 1, int pageSize = 10)
        {
            return await _apiService.GetAsync<List<Submission>>(
                $"api/v1/submissions/user?pageNumber={pageNumber}&pageSize={pageSize}"
            );
        }
    }
}

