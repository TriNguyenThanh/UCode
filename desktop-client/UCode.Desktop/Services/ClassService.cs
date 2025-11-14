using System.Collections.Generic;
using System.Threading.Tasks;
using UCode.Desktop.Models;

namespace UCode.Desktop.Services
{
    public class ClassService
    {
        private readonly ApiService _apiService;

        public ClassService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<ApiResponse<Class>> GetClassByIdAsync(string classId)
        {
            return await _apiService.GetAsync<Class>($"/api/v1/classes/{classId}");
        }

        public async Task<ApiResponse<PagedResponse<Class>>> GetMyClassesAsync(int page = 1, int pageSize = 20)
        {
            return await _apiService.GetAsync<PagedResponse<Class>>($"/api/v1/classes?page={page}&pageSize={pageSize}");
        }

        public async Task<ApiResponse<Class>> CreateClassAsync(CreateClassRequest request)
        {
            return await _apiService.PostAsync<Class>("/api/v1/classes/create", request);
        }

        public async Task<ApiResponse<Class>> UpdateClassAsync(string classId, UpdateClassRequest request)
        {
            return await _apiService.PutAsync<Class>($"/api/v1/classes/{classId}", request);
        }

        public async Task<ApiResponse<bool>> DeleteClassAsync(string classId)
        {
            return await _apiService.DeleteAsync($"/api/v1/classes/{classId}");
        }

        public async Task<ApiResponse<List<Student>>> GetClassStudentsAsync(string classId)
        {
            return await _apiService.GetAsync<List<Student>>($"/api/v1/classes/{classId}/students");
        }

        public async Task<ApiResponse<bool>> AddStudentToClassAsync(string classId, string studentId)
        {
            return await _apiService.PostAsync<bool>($"/api/v1/classes/{classId}/students/{studentId}", null);
        }

        public async Task<ApiResponse<bool>> RemoveStudentFromClassAsync(string classId, string studentId)
        {
            return await _apiService.DeleteAsync($"/api/v1/classes/remove-student?classId={classId}&studentId={studentId}");
        }

        public async Task<ApiResponse<List<User>>> GetAvailableStudentsAsync(string classId)
        {
            return await _apiService.GetAsync<List<User>>($"/api/v1/classes/{classId}/available-students");
        }

        public async Task<ApiResponse<PagedResultDto<Student>>> GetAvailableStudentsPagedAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string excludeClassId = null,
            int? year = null,
            string major = null,
            string status = null)
        {
            var queryParams = new List<string>
            {
                $"pageNumber={pageNumber}",
                $"pageSize={pageSize}"
            };

            if (!string.IsNullOrEmpty(excludeClassId))
                queryParams.Add($"excludeClassId={excludeClassId}");
            if (year.HasValue)
                queryParams.Add($"year={year.Value}");
            if (!string.IsNullOrEmpty(major))
                queryParams.Add($"major={System.Uri.EscapeDataString(major)}");
            if (!string.IsNullOrEmpty(status))
                queryParams.Add($"status={status}");

            var queryString = "?" + string.Join("&", queryParams);
            return await _apiService.GetAsync<PagedResultDto<Student>>($"/api/v1/students{queryString}");
        }

        public async Task<ApiResponse<BulkEnrollResult>> BulkEnrollStudentsAsync(string classId, List<string> studentIds)
        {
            return await _apiService.PostAsync<BulkEnrollResult>("/api/v1/classes/add-students", new { classId, studentIds });
        }

        public async Task<ApiResponse<CheckDuplicatesResult>> CheckDuplicatesAsync(string classId, List<string> identifiers)
        {
            return await _apiService.PostAsync<CheckDuplicatesResult>("/api/v1/classes/check-duplicates", new { classId, identifiers });
        }

        public async Task<ApiResponse<List<BulkValidationResult>>> ValidateStudentsBulkAsync(List<string> studentCodes)
        {
            return await _apiService.PostAsync<List<BulkValidationResult>>("/api/v1/students/validate-bulk", new { studentCodes });
        }

        public async Task<ApiResponse<BulkCreateStudentsResult>> BulkCreateStudentsAsync(List<CreateStudentRequest> students)
        {
            return await _apiService.PostAsync<BulkCreateStudentsResult>("/api/v1/students/bulk-create", new { students });
        }
    }

    public class CreateClassRequest
    {
        public string Name { get; set; } = string.Empty;
        public string ClassCode { get; set; } = string.Empty;
        public string TeacherId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class UpdateClassRequest
    {
        public string ClassName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CoverImage { get; set; } = string.Empty;
    }

    public class CreateStudentRequest
    {
        public string StudentCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Major { get; set; } = string.Empty;
        public int? ClassYear { get; set; }
    }

    public class BulkEnrollResult
    {
        public string ClassId { get; set; } = string.Empty;
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<BulkEnrollStudentResult> Results { get; set; } = new List<BulkEnrollStudentResult>();
    }

    public class BulkEnrollStudentResult
    {
        public string StudentId { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class CheckDuplicatesResult
    {
        public string ClassId { get; set; } = string.Empty;
        public int TotalChecked { get; set; }
        public int DuplicateCount { get; set; }
        public List<string> Duplicates { get; set; } = new List<string>();
    }

    public class BulkValidationResult
    {
        public string StudentCode { get; set; } = string.Empty;
        public bool Exists { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class BulkCreateStudentsResult
    {
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<BulkCreateStudentResult> Results { get; set; } = new List<BulkCreateStudentResult>();
    }

    public class BulkCreateStudentResult
    {
        public string StudentCode { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }
}
