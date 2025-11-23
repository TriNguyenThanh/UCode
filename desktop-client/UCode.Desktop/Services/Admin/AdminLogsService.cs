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
    /// Service quản lý logs cho Admin
    /// </summary>
    public class AdminLogsService : BaseApiService
    {
        public AdminLogsService(HttpClient httpClient, IDialogCoordinator dialogCoordinator, TokenStorageService tokenStorage, AuthService authService)
            : base(httpClient, dialogCoordinator, tokenStorage, authService)
        {
        }

        /// <summary>
        /// Lấy danh sách system logs với filter và pagination
        /// </summary>
        public async Task<PagedResponse<SystemLog>?> GetSystemLogsAsync(
            string? level = null,
            string? service = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? searchTerm = null,
            int pageNumber = 1,
            int pageSize = 50)
        {
            var query = $"api/v1/admin/logs/system?pageNumber={pageNumber}&pageSize={pageSize}";

            if (!string.IsNullOrEmpty(level) && level != "All")
                query += $"&level={level}";

            if (!string.IsNullOrEmpty(service) && service != "All")
                query += $"&service={Uri.EscapeDataString(service)}";

            if (startDate.HasValue)
                query += $"&startDate={startDate.Value:yyyy-MM-dd}";

            if (endDate.HasValue)
                query += $"&endDate={endDate.Value:yyyy-MM-dd}";

            if (!string.IsNullOrEmpty(searchTerm))
                query += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";

            return await GetAsync<PagedResponse<SystemLog>>(query, "tải system logs");
        }

        /// <summary>
        /// Lấy danh sách activity logs
        /// </summary>
        public async Task<PagedResponse<ActivityLog>?> GetActivityLogsAsync(
            string? userId = null,
            string? action = null,
            string? entityType = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? searchTerm = null,
            int pageNumber = 1,
            int pageSize = 50)
        {
            var query = $"api/v1/admin/logs/activity?pageNumber={pageNumber}&pageSize={pageSize}";

            if (!string.IsNullOrEmpty(userId))
                query += $"&userId={userId}";

            if (!string.IsNullOrEmpty(action) && action != "All")
                query += $"&action={Uri.EscapeDataString(action)}";

            if (!string.IsNullOrEmpty(entityType) && entityType != "All")
                query += $"&entityType={Uri.EscapeDataString(entityType)}";

            if (startDate.HasValue)
                query += $"&startDate={startDate.Value:yyyy-MM-dd}";

            if (endDate.HasValue)
                query += $"&endDate={endDate.Value:yyyy-MM-dd}";

            if (!string.IsNullOrEmpty(searchTerm))
                query += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";

            return await GetAsync<PagedResponse<ActivityLog>>(query, "tải activity logs");
        }

        /// <summary>
        /// Xuất logs ra file
        /// </summary>
        public async Task<byte[]?> ExportLogsAsync(
            string logType, // "system" or "activity"
            string? level = null,
            string? service = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string format = "xlsx")
        {
            var query = $"api/v1/admin/logs/export?logType={logType}&format={format}";

            if (!string.IsNullOrEmpty(level))
                query += $"&level={level}";

            if (!string.IsNullOrEmpty(service))
                query += $"&service={Uri.EscapeDataString(service)}";

            if (startDate.HasValue)
                query += $"&startDate={startDate.Value:yyyy-MM-dd}";

            if (endDate.HasValue)
                query += $"&endDate={endDate.Value:yyyy-MM-dd}";

            var response = await GetAsync<byte[]>(query, "xuất logs");
            return response;
        }

        /// <summary>
        /// Xóa logs cũ theo thời gian
        /// </summary>
        public async Task<bool> DeleteOldLogsAsync(DateTime beforeDate, string logType = "both")
        {
            var request = new { BeforeDate = beforeDate, LogType = logType };
            return await PostWithoutResponseAsync(
                "api/v1/admin/logs/cleanup",
                request,
                "xóa logs cũ"
            );
        }

        /// <summary>
        /// Lấy danh sách các service có trong logs
        /// </summary>
        public async Task<List<string>?> GetAvailableServicesAsync()
        {
            return await GetAsync<List<string>>(
                "api/v1/admin/logs/services",
                "tải danh sách services"
            );
        }

        /// <summary>
        /// Lấy thống kê logs
        /// </summary>
        public async Task<Dictionary<string, int>?> GetLogStatisticsAsync(
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var query = "api/v1/admin/logs/statistics";
            var first = true;

            if (startDate.HasValue)
            {
                query += $"?startDate={startDate.Value:yyyy-MM-dd}";
                first = false;
            }

            if (endDate.HasValue)
            {
                query += first ? "?" : "&";
                query += $"endDate={endDate.Value:yyyy-MM-dd}";
            }

            return await GetAsync<Dictionary<string, int>>(query, "tải thống kê logs");
        }
    }
}
