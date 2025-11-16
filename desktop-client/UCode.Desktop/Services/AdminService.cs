using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using UCode.Desktop.Models;
using UCode.Desktop.Models.Admin;

namespace UCode.Desktop.Services
{
    public class AdminService
    {
        private readonly ApiService _apiService;

        public AdminService(ApiService apiService)
        {
            _apiService = apiService;
        }

        // Dashboard
        public async Task<AdminDashboardStats?> GetDashboardStatsAsync()
        {
            System.Diagnostics.Debug.WriteLine("[AdminService] Calling GET api/v1/admin/dashboard/stats");
            var response = await _apiService.GetAsync<AdminDashboardStats>("api/v1/admin/dashboard/stats");
            System.Diagnostics.Debug.WriteLine($"[AdminService] Dashboard stats response - Success: {response?.Success}, Data: {response?.Data != null}");
            return response?.Data;
        }

        // User Management
        public async Task<List<UserManagement>> GetAllUsersAsync(
            string? searchTerm = null,
            string? role = null,
            bool? isActive = null,
            int page = 1,
            int pageSize = 20)
        {
            var query = $"api/v1/admin/users?page={page}&pageSize={pageSize}";
            
            if (!string.IsNullOrEmpty(searchTerm))
                query += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
            
            if (!string.IsNullOrEmpty(role))
                query += $"&role={role}";
            
            if (isActive.HasValue)
                query += $"&isActive={isActive.Value}";

            System.Diagnostics.Debug.WriteLine($"[AdminService] Calling GET {query}");
            var response = await _apiService.GetAsync<List<UserManagement>>(query);
            System.Diagnostics.Debug.WriteLine($"[AdminService] Users response - Success: {response?.Success}, Count: {response?.Data?.Count ?? 0}");
            return response?.Data ?? new List<UserManagement>();
        }

        public async Task<UserManagement?> GetUserByIdAsync(Guid userId)
        {
            var response = await _apiService.GetAsync<UserManagement>($"api/v1/admin/users/{userId}");
            return response?.Data;
        }

        public async Task<bool> CreateUserAsync(CreateUserRequest request)
        {
            var response = await _apiService.PostAsync<CreateUserRequest, ApiResponse<object>>(
                "api/v1/admin/users", request);
            return response?.Success ?? false;
        }

        public async Task<bool> UpdateUserAsync(Guid userId, UpdateUserRequest request)
        {
            var response = await _apiService.PutAsync<UpdateUserRequest, ApiResponse<object>>(
                $"api/v1/admin/users/{userId}", request);
            return response?.Success ?? false;
        }

        public async Task<bool> DeleteUserAsync(Guid userId)
        {
            var response = await _apiService.DeleteAsync<ApiResponse<object>>(
                $"api/v1/admin/users/{userId}");
            return response?.Success ?? false;
        }

        public async Task<bool> ResetUserPasswordAsync(Guid userId, string newPassword)
        {
            var request = new { Password = newPassword };
            var response = await _apiService.PostAsync<object, ApiResponse<object>>(
                $"api/v1/admin/users/{userId}/reset-password", request);
            return response?.Success ?? false;
        }

        public async Task<bool> BulkUserActionAsync(BulkUserActionRequest request)
        {
            var response = await _apiService.PostAsync<BulkUserActionRequest, ApiResponse<object>>(
                "api/v1/admin/users/bulk-action", request);
            return response?.Success ?? false;
        }

        // Class Management
        public async Task<List<ClassManagement>> GetAllClassesAsync(
            string? searchTerm = null,
            Guid? teacherId = null,
            bool? isActive = null,
            int page = 1,
            int pageSize = 20)
        {
            var query = $"api/v1/admin/classes?page={page}&pageSize={pageSize}";
            
            if (!string.IsNullOrEmpty(searchTerm))
                query += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
            
            if (teacherId.HasValue)
                query += $"&teacherId={teacherId.Value}";
            
            if (isActive.HasValue)
                query += $"&isActive={isActive.Value}";

            var response = await _apiService.GetAsync<List<ClassManagement>>(query);
            return response?.Data ?? new List<ClassManagement>();
        }

        public async Task<ClassManagement?> GetClassByIdAsync(Guid classId)
        {
            var response = await _apiService.GetAsync<ClassManagement>($"api/v1/admin/classes/{classId}");
            return response?.Data;
        }

        public async Task<ClassStatistics?> GetClassStatisticsAsync(Guid classId)
        {
            var response = await _apiService.GetAsync<ClassStatistics>(
                $"api/v1/admin/classes/{classId}/statistics");
            return response?.Data;
        }

        public async Task<bool> CreateClassAsync(Models.Admin.CreateClassRequest request)
        {
            var response = await _apiService.PostAsync<Models.Admin.CreateClassRequest, ApiResponse<object>>(
                "api/v1/admin/classes", request);
            return response?.Success ?? false;
        }

        public async Task<bool> UpdateClassAsync(Guid classId, Models.Admin.UpdateClassRequest request)
        {
            var response = await _apiService.PutAsync<Models.Admin.UpdateClassRequest, ApiResponse<object>>(
                $"api/v1/admin/classes/{classId}", request);
            return response?.Success ?? false;
        }

        public async Task<bool> DeleteClassAsync(Guid classId)
        {
            var response = await _apiService.DeleteAsync<ApiResponse<object>>(
                $"api/v1/admin/classes/{classId}");
            return response?.Success ?? false;
        }

        public async Task<bool> ReassignClassTeacherAsync(Guid classId, Guid newTeacherId)
        {
            var request = new { TeacherId = newTeacherId };
            var response = await _apiService.PutAsync<object, ApiResponse<object>>(
                $"api/v1/admin/classes/{classId}/teacher", request);
            return response?.Success ?? false;
        }

        // Problem Management
        public async Task<List<ProblemManagement>> GetAllProblemsAsync(
            string? searchTerm = null,
            string? difficulty = null,
            AdminProblemStatus? status = null,
            ProblemVisibility? visibility = null,
            int page = 1,
            int pageSize = 20)
        {
            var query = $"api/v1/admin/problems?page={page}&pageSize={pageSize}";
            
            if (!string.IsNullOrEmpty(searchTerm))
                query += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
            
            if (!string.IsNullOrEmpty(difficulty))
                query += $"&difficulty={difficulty}";
            
            if (status.HasValue)
                query += $"&status={status.Value}";
            
            if (visibility.HasValue)
                query += $"&visibility={visibility.Value}";

            var response = await _apiService.GetAsync<List<ProblemManagement>>(query);
            return response?.Data ?? new List<ProblemManagement>();
        }

        public async Task<bool> ApproveProblemAsync(ApproveProblemRequest request)
        {
            var response = await _apiService.PostAsync<ApproveProblemRequest, ApiResponse<object>>(
                "api/v1/admin/problems/approve", request);
            return response?.Success ?? false;
        }

        public async Task<bool> BulkProblemActionAsync(BulkProblemActionRequest request)
        {
            var response = await _apiService.PostAsync<BulkProblemActionRequest, ApiResponse<object>>(
                "api/v1/admin/problems/bulk-action", request);
            return response?.Success ?? false;
        }

        public async Task<bool> DeleteProblemAsync(Guid problemId)
        {
            var response = await _apiService.DeleteAsync<ApiResponse<object>>(
                $"api/v1/admin/problems/{problemId}");
            return response?.Success ?? false;
        }

        // Settings
        public async Task<SystemSettings?> GetSystemSettingsAsync()
        {
            var response = await _apiService.GetAsync<SystemSettings>("api/v1/admin/settings");
            return response?.Data;
        }

        public async Task<bool> UpdateSystemSettingsAsync(SystemSettings settings)
        {
            var response = await _apiService.PutAsync<SystemSettings, ApiResponse<object>>(
                "api/v1/admin/settings", settings);
            return response?.Success ?? false;
        }

        // Export functions
        public async Task<byte[]?> ExportUsersToExcelAsync()
        {
            return await _apiService.GetBytesAsync("api/v1/admin/users/export/excel");
        }

        public async Task<byte[]?> ExportClassesToExcelAsync()
        {
            return await _apiService.GetBytesAsync("api/v1/admin/classes/export/excel");
        }

        public async Task<byte[]?> ExportProblemsToExcelAsync()
        {
            return await _apiService.GetBytesAsync("api/v1/admin/problems/export/excel");
        }
    }
}
