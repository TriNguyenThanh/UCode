using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;
using UCode.Desktop.Models;
using UCode.Desktop.Models.Admin;

namespace UCode.Desktop.Services.Admin
{
    /// <summary>
    /// Service quản lý classes cho Admin
    /// </summary>
    public class AdminClassService : BaseApiService
    {
        public AdminClassService(HttpClient httpClient, IDialogCoordinator dialogCoordinator, TokenStorageService tokenStorage, AuthService authService)
            : base(httpClient, dialogCoordinator, tokenStorage, authService)
        {
        }

        /// <summary>
        /// Lấy tất cả classes trong hệ thống (không filter theo teacher)
        /// </summary>
        public async Task<PagedResponse<AdminClassResponse>?> GetAllClassesAsync(
            string? teacherId = null,
            bool? isActive = null,
            bool? isArchived = null,
            string? searchTerm = null,
            int pageNumber = 1,
            int pageSize = 20)
        {
            var query = $"api/v1/admin/classes?pageNumber={pageNumber}&pageSize={pageSize}";

            if (!string.IsNullOrEmpty(teacherId))
                query += $"&teacherId={teacherId}";

            if (isActive.HasValue)
                query += $"&isActive={isActive.Value}";

            if (isArchived.HasValue)
                query += $"&isArchived={isArchived.Value}";

            if (!string.IsNullOrEmpty(searchTerm))
                query += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";

            return await GetAsync<PagedResponse<AdminClassResponse>>(query, "tải danh sách lớp học");
        }

        /// <summary>
        /// Lấy chi tiết class
        /// </summary>
        public async Task<ClassDetailAdmin?> GetClassDetailAsync(string classId)
        {
            return await GetAsync<ClassDetailAdmin>(
                $"api/v1/admin/classes/{classId}",
                "tải thông tin chi tiết lớp học"
            );
        }

        /// <summary>
        /// Lấy danh sách students trong class
        /// </summary>
        public async Task<PagedResponse<Student>?> GetClassStudentsAsync(
            string classId,
            int pageNumber = 1,
            int pageSize = 50,
            string? searchTerm = null)
        {
            var query = $"api/v1/admin/classes/{classId}/students?pageNumber={pageNumber}&pageSize={pageSize}";

            if (!string.IsNullOrEmpty(searchTerm))
                query += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";

            return await GetAsync<PagedResponse<Student>>(query, "tải danh sách sinh viên");
        }

        /// <summary>
        /// Cập nhật thông tin class (bao gồm cả reassign teacher)
        /// </summary>
        public async Task<bool> UpdateClassAsync(UpdateClassByAdminRequest request)
        {
            return await PutWithoutResponseAsync(
                $"api/v1/admin/classes/{request.ClassId}",
                request,
                "cập nhật lớp học"
            );
        }

        /// <summary>
        /// Archive class
        /// </summary>
        public async Task<bool> ArchiveClassAsync(string classId, string? reason = null)
        {
            var request = new ArchiveClassRequest { Reason = reason };
            return await PatchAsync<object>(
                $"api/v1/admin/classes/{classId}/archive",
                request,
                "archive lớp học"
            ) != null;
        }

        /// <summary>
        /// Unarchive class
        /// </summary>
        public async Task<bool> UnarchiveClassAsync(string classId)
        {
            return await PatchAsync<object>(
                $"api/v1/admin/classes/{classId}/unarchive",
                new { },
                "unarchive lớp học"
            ) != null;
        }

        /// <summary>
        /// Xóa vĩnh viễn class (force delete)
        /// </summary>
        public async Task<bool> DeleteClassAsync(string classId)
        {
            return await DeleteAsync(
                $"api/v1/admin/classes/{classId}",
                "xóa lớp học"
            );
        }

        /// <summary>
        /// Bulk action trên nhiều classes
        /// </summary>
        public async Task<BulkActionResult?> BulkActionAsync(
            string action,
            List<string> classIds,
            string? reason = null)
        {
            var request = new BulkActionRequest
            {
                Action = action,
                ClassIds = classIds,
                Reason = reason
            };

            return await PostAsync<BulkActionResult>(
                "api/v1/admin/classes/bulk-action",
                request,
                $"thực hiện {action} trên {classIds.Count} lớp học"
            );
        }

        /// <summary>
        /// Lấy thống kê classes
        /// </summary>
        public async Task<ClassStatistics?> GetClassStatisticsAsync()
        {
            return await GetAsync<ClassStatistics>(
                "api/v1/admin/classes/statistics",
                "tải thống kê lớp học"
            );
        }

        /// <summary>
        /// Reassign teacher cho class
        /// </summary>
        public async Task<bool> ReassignTeacherAsync(string classId, string newTeacherId)
        {
            var request = new UpdateClassByAdminRequest
            {
                ClassId = Guid.Parse(classId),
                TeacherId = newTeacherId
            };

            return await UpdateClassAsync(request);
        }
    }
}
