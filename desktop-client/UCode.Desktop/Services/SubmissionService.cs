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
                $"/api/v1/submissions/assignment/{assignmentId}/problem/{problemId}/best?pageNumber={pageNumber}&pageSize={pageSize}"
            );
        }

        public async Task<ApiResponse<BestSubmission>> GetBestSubmissionForStudentAsync(string assignmentId, string problemId, string userId)
        {
            return await _apiService.GetAsync<BestSubmission>(
                $"/api/v1/submissions/assignment/{assignmentId}/problem/{problemId}/student/{userId}/best"
            );
        }

        public async Task<ApiResponse<Submission>> GetSubmissionAsync(string submissionId)
        {
            return await _apiService.GetAsync<Submission>($"/api/v1/submissions/{submissionId}");
        }
    }
}

