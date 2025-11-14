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
            return await _apiService.DeleteAsync($"/api/v1/classes/{classId}/students/{studentId}");
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
}

