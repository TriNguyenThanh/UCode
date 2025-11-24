using System.Net.Http;
using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;
using UCode.Desktop.Models.Admin;

namespace UCode.Desktop.Services.Admin
{
    /// <summary>
    /// Service quản lý settings cho Admin
    /// </summary>
    public class AdminSettingsService : BaseApiService
    {
        public AdminSettingsService(HttpClient httpClient, IDialogCoordinator dialogCoordinator, TokenStorageService tokenStorage, AuthService authService)
            : base(httpClient, dialogCoordinator, tokenStorage, authService)
        {
        }

        /// <summary>
        /// Lấy tất cả system settings
        /// </summary>
        public async Task<SystemSettings?> GetSystemSettingsAsync()
        {
            return await GetAsync<SystemSettings>(
                "api/v1/admin/settings",
                "tải cài đặt hệ thống"
            );
        }

        /// <summary>
        /// Cập nhật general settings
        /// </summary>
        public async Task<bool> UpdateGeneralSettingsAsync(GeneralSettings settings)
        {
            return await PutWithoutResponseAsync(
                "api/v1/admin/settings/general",
                settings,
                "cập nhật cài đặt chung"
            );
        }

        /// <summary>
        /// Cập nhật security settings
        /// </summary>
        public async Task<bool> UpdateSecuritySettingsAsync(SecuritySettings settings)
        {
            return await PutWithoutResponseAsync(
                "api/v1/admin/settings/security",
                settings,
                "cập nhật cài đặt bảo mật"
            );
        }

        /// <summary>
        /// Cập nhật email settings
        /// </summary>
        public async Task<bool> UpdateEmailSettingsAsync(EmailSettings settings)
        {
            return await PutWithoutResponseAsync(
                "api/v1/admin/settings/email",
                settings,
                "cập nhật cài đặt email"
            );
        }

        /// <summary>
        /// Cập nhật storage settings
        /// </summary>
        public async Task<bool> UpdateStorageSettingsAsync(StorageSettings settings)
        {
            return await PutWithoutResponseAsync(
                "api/v1/admin/settings/storage",
                settings,
                "cập nhật cài đặt lưu trữ"
            );
        }

        /// <summary>
        /// Test email configuration
        /// </summary>
        public async Task<bool> TestEmailConfigurationAsync(string testEmail)
        {
            var request = new { Email = testEmail };
            return await PostWithoutResponseAsync(
                "api/v1/admin/settings/email/test",
                request,
                "kiểm tra cấu hình email"
            );
        }

        /// <summary>
        /// Reset settings về mặc định
        /// </summary>
        public async Task<bool> ResetToDefaultAsync(string section)
        {
            return await PostWithoutResponseAsync(
                $"api/v1/admin/settings/{section}/reset",
                new { },
                $"khôi phục cài đặt {section} về mặc định"
            );
        }

        /// <summary>
        /// Backup system settings
        /// </summary>
        public async Task<byte[]?> BackupSettingsAsync()
        {
            return await GetAsync<byte[]>(
                "api/v1/admin/settings/backup",
                "sao lưu cài đặt hệ thống"
            );
        }

        /// <summary>
        /// Restore system settings
        /// </summary>
        public async Task<bool> RestoreSettingsAsync(byte[] backupData)
        {
            return await PostWithoutResponseAsync(
                "api/v1/admin/settings/restore",
                backupData,
                "khôi phục cài đặt hệ thống"
            );
        }
    }
}
