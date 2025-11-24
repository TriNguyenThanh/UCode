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
            return await _apiService.PutAsync<BestSubmission>($"/api/v1/submissions/update-score", request);
        }

        // Student methods
        public async Task<ApiResponse<AssignmentUser>> GetMyAssignmentDetailAsync(string assignmentId)
        {
            return await _apiService.GetAsync<AssignmentUser>($"/api/v1/assignments/{assignmentId}/student/my-detail");
        }

        public async Task<ApiResponse<AssignmentUser>> StartAssignmentAsync(string assignmentId)
        {
            return await _apiService.PostAsync<AssignmentUser>($"/api/v1/assignments/{assignmentId}/student/start", null);
        }

        public async Task<ApiResponse<List<Assignment>>> GetStudentAssignmentsAsync()
        {
            return await _apiService.GetAsync<List<Assignment>>("/api/v1/assignments/student/my-assignments");
        }

        public async Task<ApiResponse<List<BestSubmission>>> GetBestSubmissionsAsync(string assignmentId, List<string> problemIds)
        {
            var requestBody = new { problemIds = problemIds };
            return await _apiService.PostAsync<List<BestSubmission>>($"/api/v1/submissions/assignment/{assignmentId}/problem/list-my-best", requestBody);
        }
        

        public async Task NotifyAssignmentPublishedAsync(string assignmentId, string teacherId)
        {
            try
            {
                // Webhook URL of the Zalo Bot Service
                var webhookUrl = "http://localhost:3000/api/webhook/assignment-published";
                
                var payload = new
                {
                    assignmentId = assignmentId,
                    teacherId = teacherId
                };

                using (var client = new System.Net.Http.HttpClient())
                {
                    var json = System.Text.Json.JsonSerializer.Serialize(payload);
                    var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    
                    // Fire and forget - don't wait for response to block UI
                    // But here we await to ensure it's sent, but catch exceptions
                    var response = await client.PostAsync(webhookUrl, content);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Webhook] Sent notification for assignment {assignmentId}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[Webhook] Failed to send notification. Status: {response.StatusCode}");
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Webhook] Exception sending notification: {ex.Message}");
            }
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
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool AllowLateSubmission { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<AssignmentProblem> Problems { get; set; } = new();
    }

    // AssignmentProblem moved to Models/Assignment.cs to avoid ambiguity

    public class GradeSubmissionRequest
    {
        public string SubmissionId { get; set; }
        public int NewScore { get; set; }
        public string Comment { get; set; } = string.Empty;
    }

    public class AssignmentProblemDetail
    {
        public string ProblemId { get; set; } = string.Empty;
        public int Points { get; set; }
        public int OrderIndex { get; set; }
    }
}

