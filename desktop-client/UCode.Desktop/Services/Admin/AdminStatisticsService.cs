using System;
using System.Net.Http;
using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;
using UCode.Desktop.Models.Admin;

namespace UCode.Desktop.Services.Admin
{
    /// <summary>
    /// Service để lấy statistics cho Admin Dashboard
    /// </summary>
    public class AdminStatisticsService : BaseApiService
    {
        public AdminStatisticsService(HttpClient httpClient, IDialogCoordinator dialogCoordinator, TokenStorageService tokenStorage, AuthService authService)
            : base(httpClient, dialogCoordinator, tokenStorage, authService)
        {
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
        /// Lấy thống kê bài tập/bài toán/bài nộp từ assignment service
        /// </summary>
        public async Task<AssignmentSystemStatisticsResponse?> GetAssignmentSystemStatisticsAsync()
        {
            return await GetAsync<AssignmentSystemStatisticsResponse>(
                "api/v1/assignments/statistics/system",
                "tải thống kê bài tập"
            );
        }

        /// <summary>
        /// Lấy thống kê tổng quan hệ thống
        /// Kết hợp từ nhiều API calls
        /// </summary>
        public async Task<SystemStatistics?> GetSystemStatisticsAsync()
        {
            var userStats = await GetUserStatisticsAsync();
            var classStats = await GetClassStatisticsAsync();

            if (userStats == null || classStats == null)
                return null;

            // Combine statistics
            return new SystemStatistics
            {
                TotalUsers = userStats.TotalUsers,
                TotalStudents = userStats.StudentCount,
                TotalTeachers = userStats.TeacherCount,
                TotalAdmins = userStats.AdminCount,
                TotalClasses = classStats.TotalClasses,
                TotalActiveClasses = classStats.ActiveClasses,
                TotalArchivedClasses = classStats.ArchivedClasses,
                // TODO: Add problems and submissions stats when APIs are available
                TotalProblems = 0,
                TotalSubmissions = 0,
                TodayActiveUsers = 0,
                WeekActiveUsers = 0,
                MonthActiveUsers = 0
            };
        }
    }
}
