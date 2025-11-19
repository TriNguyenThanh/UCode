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
    /// Service quản lý users cho Admin
    /// </summary>
    public class AdminUserService : BaseApiService
    {
        public AdminUserService(HttpClient httpClient, IDialogCoordinator dialogCoordinator, TokenStorageService tokenStorage, AuthService authService)
            : base(httpClient, dialogCoordinator, tokenStorage, authService)
        {
        }

        /// <summary>
        /// Lấy danh sách tất cả người dùng với filter và pagination
        /// </summary>
        public async Task<PagedResponse<AdminUserResponse>?> GetAllUsersAsync(
            string? role = null,
            bool? isActive = null,
            string? searchTerm = null,
            int pageNumber = 1,
            int pageSize = 20)
        {
            var query = $"api/v1/admin/users?pageNumber={pageNumber}&pageSize={pageSize}";

            if (!string.IsNullOrEmpty(role) && role != "All")
                query += $"&role={role}";

            if (isActive.HasValue)
                query += $"&isActive={isActive.Value}";

            if (!string.IsNullOrEmpty(searchTerm))
                query += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";

            return await GetAsync<PagedResponse<AdminUserResponse>>(query, "tải danh sách người dùng");
        }

        /// <summary>
        /// Lấy chi tiết một người dùng
        /// </summary>
        public async Task<UserDetailAdmin?> GetUserDetailAsync(string userId)
        {
            return await GetAsync<UserDetailAdmin>(
                $"api/v1/admin/users/{userId}",
                "tải thông tin chi tiết người dùng"
            );
        }

        /// <summary>
        /// Tạo người dùng mới (admin có thể tạo bất kỳ role nào)
        /// </summary>
        public async Task<User?> CreateUserAsync(CreateUserByAdminRequest request)
        {
            return await PostAsync<User>(
                "api/v1/admin/users",
                request,
                "tạo người dùng mới"
            );
        }

        /// <summary>
        /// Cập nhật thông tin người dùng
        /// </summary>
        public async Task<User?> UpdateUserAsync(string userId, UpdateUserByAdminRequest request)
        {
            return await PutAsync<User>(
                $"api/v1/admin/users/{userId}",
                request,
                "cập nhật thông tin người dùng"
            );
        }

        /// <summary>
        /// Xóa người dùng
        /// </summary>
        public async Task<bool> DeleteUserAsync(string userId)
        {
            return await DeleteAsync(
                $"api/v1/admin/users/{userId}",
                "xóa người dùng"
            );
        }

        /// <summary>
        /// Bulk action trên nhiều người dùng
        /// </summary>
        public async Task<BulkActionResult?> BulkActionAsync(
            string action,
            List<string> userIds,
            string? newRole = null,
            string? reason = null)
        {
            var request = new BulkUserActionRequest
            {
                Action = action,
                UserIds = userIds,
                NewRole = newRole,
                Reason = reason
            };

            return await PostAsync<BulkActionResult>(
                "api/v1/admin/users/bulk-action",
                request,
                $"thực hiện {action} trên {userIds.Count} người dùng"
            );
        }

        /// <summary>
        /// Lấy thống kê người dùng
        /// </summary>
        public async Task<UserStatistics?> GetUserStatisticsAsync()
        {
            return await GetAsync<UserStatistics>(
                "api/v1/admin/users/statistics",
                "tải thống kê người dùng"
            );
        }

        /// <summary>
        /// Lấy activity logs của một người dùng
        /// </summary>
        public async Task<List<ActivityLog>?> GetUserActivityLogsAsync(
            string userId,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int pageNumber = 1,
            int pageSize = 50)
        {
            var query = $"api/v1/admin/users/{userId}/activity-logs?pageNumber={pageNumber}&pageSize={pageSize}";

            if (startDate.HasValue)
                query += $"&startDate={startDate.Value:yyyy-MM-dd}";

            if (endDate.HasValue)
                query += $"&endDate={endDate.Value:yyyy-MM-dd}";

            var result = await GetAsync<PagedResponse<ActivityLog>>(query, "tải lịch sử hoạt động");
            return result?.Items;
        }

        /// <summary>
        /// Reset password cho user
        /// </summary>
        public async Task<bool> ResetPasswordAsync(string userId, string newPassword)
        {
            var request = new { NewPassword = newPassword };
            return await PostWithoutResponseAsync(
                $"api/v1/admin/users/{userId}/reset-password",
                request,
                "đặt lại mật khẩu"
            );
        }

        /// <summary>
        /// Ban user
        /// </summary>
        public async Task<bool> BanUserAsync(string userId, string reason)
        {
            var request = new { Reason = reason };
            return await PostWithoutResponseAsync(
                $"api/v1/admin/users/{userId}/ban",
                request,
                "cấm người dùng"
            );
        }

        /// <summary>
        /// Unban user
        /// </summary>
        public async Task<bool> UnbanUserAsync(string userId)
        {
            return await PostWithoutResponseAsync(
                $"api/v1/admin/users/{userId}/unban",
                new { },
                "bỏ cấm người dùng"
            );
        }
    }
}
