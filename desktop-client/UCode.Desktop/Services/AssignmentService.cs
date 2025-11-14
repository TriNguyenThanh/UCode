using System.Collections.Generic;
using System.Threading.Tasks;
using UCode.Desktop.Models;

namespace UCode.Desktop.Services
{
    public class AssignmentService
    {
        private readonly ApiService _apiService;

        public AssignmentService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<ApiResponse<Assignment>> GetAssignmentAsync(string assignmentId)
        {
            return await _apiService.GetAsync<Assignment>($"/api/v1/assignments/{assignmentId}");
        }

        public async Task<ApiResponse<List<Assignment>>> GetMyAssignmentsAsync()
        {
            return await _apiService.GetAsync<List<Assignment>>("/api/v1/assignments/my-assignments");
        }

        public async Task<ApiResponse<List<Assignment>>> GetAssignmentsByClassAsync(string classId)
        {
            return await _apiService.GetAsync<List<Assignment>>($"/api/v1/assignments/class/{classId}");
        }

        public async Task<ApiResponse<Assignment>> CreateAssignmentAsync(CreateAssignmentRequest request)
        {
            return await _apiService.PostAsync<Assignment>("/api/v1/assignments/create", request);
        }

        public async Task<ApiResponse<Assignment>> UpdateAssignmentAsync(string assignmentId, UpdateAssignmentRequest request)
        {
            return await _apiService.PutAsync<Assignment>($"/api/v1/assignments/update/{assignmentId}", request);
        }

        public async Task<ApiResponse<bool>> DeleteAssignmentAsync(string assignmentId)
        {
            return await _apiService.DeleteAsync($"/api/v1/assignments/delete/{assignmentId}");
        }

        public async Task<ApiResponse<List<AssignmentUser>>> GetAssignmentStudentsAsync(string assignmentId)
        {
            return await _apiService.GetAsync<List<AssignmentUser>>($"/api/v1/assignments/{assignmentId}/students");
        }

        public async Task<ApiResponse<AssignmentStatistics>> GetAssignmentStatisticsAsync(string assignmentId)
        {
            return await _apiService.GetAsync<AssignmentStatistics>($"/api/v1/assignments/{assignmentId}/statistics");
        }

        public async Task<ApiResponse<BestSubmission>> GradeSubmissionAsync(string assignmentId, string submissionId, GradeSubmissionRequest request)
        {
            return await _apiService.PutAsync<BestSubmission>($"/api/v1/assignments/{assignmentId}/grade-submission/{submissionId}", request);
        }
    }

    // public class CreateAssignmentRequest
    // {
    //     public string AssignmentType { get; set; } = "HOMEWORK";
    //     public string ClassId { get; set; } = string.Empty;
    //     public string Title { get; set; } = string.Empty;
    //     public string Description { get; set; } = string.Empty;
    //     public string StartTime { get; set; } = string.Empty;
    //     public string EndTime { get; set; } = string.Empty;
    //     public bool AllowLateSubmission { get; set; }
    //     public string Status { get; set; } = "DRAFT";
    //     public List<AssignmentProblem> Problems { get; set; } = new();
    // }

    public class UpdateAssignmentRequest
    {
        public string AssignmentType { get; set; } = string.Empty;
        public string ClassId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public bool AllowLateSubmission { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<AssignmentProblem> Problems { get; set; } = new();
    }

    // AssignmentProblem moved to Models/Assignment.cs to avoid ambiguity

    public class GradeSubmissionRequest
    {
        public double? Score { get; set; }
        public string TeacherFeedback { get; set; } = string.Empty;
    }

    public class AssignmentProblemDetail
    {
        public string ProblemId { get; set; } = string.Empty;
        public int Points { get; set; }
        public int OrderIndex { get; set; }
    }
}

