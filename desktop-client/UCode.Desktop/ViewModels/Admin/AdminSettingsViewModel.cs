using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using UCode.Desktop.Helpers;
using UCode.Desktop.Models.Admin;
using UCode.Desktop.Services.Admin;

namespace UCode.Desktop.ViewModels.Admin
{
    /// <summary>
    /// ViewModel cho Admin Settings Management
    /// </summary>
    public class AdminSettingsViewModel : INotifyPropertyChanged
    {
        private readonly AdminSettingsService _settingsService;
        private readonly IDialogCoordinator _dialogCoordinator;

        private bool _isLoading;
        private SystemSettings? _settings;
        private string _selectedSection = "General";

        // General Settings
        private string _systemName = "UCode";
        private string _systemDescription = string.Empty;
        private string _supportEmail = string.Empty;
        private bool _maintenanceMode = false;
        private string _maintenanceMessage = string.Empty;

        // Security Settings
        private bool _requireEmailVerification = true;
        private int _sessionTimeout = 30;
        private int _maxLoginAttempts = 5;
        private int _lockoutDuration = 15;
        private bool _enableTwoFactor = false;

        // Email Settings
        private string _smtpHost = string.Empty;
        private int _smtpPort = 587;
        private string _smtpUsername = string.Empty;
        private string _smtpPassword = string.Empty;
        private string _fromEmail = string.Empty;
        private string _fromName = string.Empty;
        private bool _enableSsl = true;

        // Storage Settings
        private long _maxFileSize = 10485760; // 10MB
        private string _allowedExtensions = ".jpg,.png,.pdf,.zip";
        private string _storageProvider = "Local";
        private long _totalStorageQuota = 107374182400; // 100GB

        private bool _hasUnsavedChanges = false;

        public string[] Sections { get; } = { "General", "Security", "Email", "Storage" };

        #region Properties

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string SelectedSection
        {
            get => _selectedSection;
            set => SetProperty(ref _selectedSection, value);
        }

        public bool HasUnsavedChanges
        {
            get => _hasUnsavedChanges;
            set => SetProperty(ref _hasUnsavedChanges, value);
        }

        // General Settings Properties
        public string SystemName
        {
            get => _systemName;
            set
            {
                if (SetProperty(ref _systemName, value))
                    HasUnsavedChanges = true;
            }
        }

        public string SystemDescription
        {
            get => _systemDescription;
            set
            {
                if (SetProperty(ref _systemDescription, value))
                    HasUnsavedChanges = true;
            }
        }

        public string SupportEmail
        {
            get => _supportEmail;
            set
            {
                if (SetProperty(ref _supportEmail, value))
                    HasUnsavedChanges = true;
            }
        }

        public bool MaintenanceMode
        {
            get => _maintenanceMode;
            set
            {
                if (SetProperty(ref _maintenanceMode, value))
                    HasUnsavedChanges = true;
            }
        }

        public string MaintenanceMessage
        {
            get => _maintenanceMessage;
            set
            {
                if (SetProperty(ref _maintenanceMessage, value))
                    HasUnsavedChanges = true;
            }
        }

        // Security Settings Properties
        public bool RequireEmailVerification
        {
            get => _requireEmailVerification;
            set
            {
                if (SetProperty(ref _requireEmailVerification, value))
                    HasUnsavedChanges = true;
            }
        }

        public int SessionTimeout
        {
            get => _sessionTimeout;
            set
            {
                if (SetProperty(ref _sessionTimeout, value))
                    HasUnsavedChanges = true;
            }
        }

        public int MaxLoginAttempts
        {
            get => _maxLoginAttempts;
            set
            {
                if (SetProperty(ref _maxLoginAttempts, value))
                    HasUnsavedChanges = true;
            }
        }

        public int LockoutDuration
        {
            get => _lockoutDuration;
            set
            {
                if (SetProperty(ref _lockoutDuration, value))
                    HasUnsavedChanges = true;
            }
        }

        public bool EnableTwoFactor
        {
            get => _enableTwoFactor;
            set
            {
                if (SetProperty(ref _enableTwoFactor, value))
                    HasUnsavedChanges = true;
            }
        }

        // Email Settings Properties
        public string SmtpHost
        {
            get => _smtpHost;
            set
            {
                if (SetProperty(ref _smtpHost, value))
                    HasUnsavedChanges = true;
            }
        }

        public int SmtpPort
        {
            get => _smtpPort;
            set
            {
                if (SetProperty(ref _smtpPort, value))
                    HasUnsavedChanges = true;
            }
        }

        public string SmtpUsername
        {
            get => _smtpUsername;
            set
            {
                if (SetProperty(ref _smtpUsername, value))
                    HasUnsavedChanges = true;
            }
        }

        public string SmtpPassword
        {
            get => _smtpPassword;
            set
            {
                if (SetProperty(ref _smtpPassword, value))
                    HasUnsavedChanges = true;
            }
        }

        public string FromEmail
        {
            get => _fromEmail;
            set
            {
                if (SetProperty(ref _fromEmail, value))
                    HasUnsavedChanges = true;
            }
        }

        public string FromName
        {
            get => _fromName;
            set
            {
                if (SetProperty(ref _fromName, value))
                    HasUnsavedChanges = true;
            }
        }

        public bool EnableSsl
        {
            get => _enableSsl;
            set
            {
                if (SetProperty(ref _enableSsl, value))
                    HasUnsavedChanges = true;
            }
        }

        // Storage Settings Properties
        public long MaxFileSize
        {
            get => _maxFileSize;
            set
            {
                if (SetProperty(ref _maxFileSize, value))
                    HasUnsavedChanges = true;
            }
        }

        public string MaxFileSizeMB
        {
            get => (_maxFileSize / 1048576.0).ToString("F2");
            set
            {
                if (double.TryParse(value, out var mb))
                {
                    MaxFileSize = (long)(mb * 1048576);
                }
            }
        }

        public string AllowedExtensions
        {
            get => _allowedExtensions;
            set
            {
                if (SetProperty(ref _allowedExtensions, value))
                    HasUnsavedChanges = true;
            }
        }

        public string StorageProvider
        {
            get => _storageProvider;
            set
            {
                if (SetProperty(ref _storageProvider, value))
                    HasUnsavedChanges = true;
            }
        }

        public long TotalStorageQuota
        {
            get => _totalStorageQuota;
            set
            {
                if (SetProperty(ref _totalStorageQuota, value))
                    HasUnsavedChanges = true;
            }
        }

        public string TotalStorageQuotaGB
        {
            get => (_totalStorageQuota / 1073741824.0).ToString("F2");
            set
            {
                if (double.TryParse(value, out var gb))
                {
                    TotalStorageQuota = (long)(gb * 1073741824);
                }
            }
        }

        #endregion

        #region Commands

        public ICommand LoadSettingsCommand { get; }
        public ICommand SaveGeneralSettingsCommand { get; }
        public ICommand SaveSecuritySettingsCommand { get; }
        public ICommand SaveEmailSettingsCommand { get; }
        public ICommand SaveStorageSettingsCommand { get; }
        public ICommand TestEmailCommand { get; }
        public ICommand ResetToDefaultCommand { get; }
        public ICommand BackupSettingsCommand { get; }
        public ICommand RefreshCommand { get; }

        #endregion

        public AdminSettingsViewModel(
            AdminSettingsService settingsService,
            IDialogCoordinator dialogCoordinator)
        {
            _settingsService = settingsService;
            _dialogCoordinator = dialogCoordinator;

            LoadSettingsCommand = new RelayCommand(async _ => await LoadSettingsAsync());
            SaveGeneralSettingsCommand = new RelayCommand(async _ => await SaveGeneralSettingsAsync());
            SaveSecuritySettingsCommand = new RelayCommand(async _ => await SaveSecuritySettingsAsync());
            SaveEmailSettingsCommand = new RelayCommand(async _ => await SaveEmailSettingsAsync());
            SaveStorageSettingsCommand = new RelayCommand(async _ => await SaveStorageSettingsAsync());
            TestEmailCommand = new RelayCommand(async _ => await TestEmailConfigurationAsync());
            ResetToDefaultCommand = new RelayCommand(async _ => await ResetToDefaultAsync());
            BackupSettingsCommand = new RelayCommand(async _ => await BackupSettingsAsync());
            RefreshCommand = new RelayCommand(async _ => await LoadSettingsAsync());
        }

        public async Task InitializeAsync()
        {
            await LoadSettingsAsync();
        }

        public async Task LoadSettingsAsync()
        {
            IsLoading = true;

            try
            {
                _settings = await _settingsService.GetSystemSettingsAsync();

                if (_settings != null)
                {
                    // General
                    SystemName = _settings.General.SystemName;
                    SystemDescription = _settings.General.SystemDescription;
                    SupportEmail = _settings.General.SupportEmail;
                    MaintenanceMode = _settings.General.MaintenanceMode;
                    MaintenanceMessage = _settings.General.MaintenanceMessage ?? string.Empty;

                    // Security
                    RequireEmailVerification = _settings.Security.RequireEmailVerification;
                    SessionTimeout = _settings.Security.SessionTimeout;
                    MaxLoginAttempts = _settings.Security.MaxLoginAttempts;
                    LockoutDuration = _settings.Security.LockoutDuration;
                    EnableTwoFactor = _settings.Security.EnableTwoFactor;

                    // Email
                    SmtpHost = _settings.Email.SmtpHost;
                    SmtpPort = _settings.Email.SmtpPort;
                    SmtpUsername = _settings.Email.SmtpUsername;
                    SmtpPassword = _settings.Email.SmtpPassword;
                    FromEmail = _settings.Email.FromEmail;
                    FromName = _settings.Email.FromName;
                    EnableSsl = _settings.Email.EnableSsl;

                    // Storage
                    MaxFileSize = _settings.Storage.MaxFileSize;
                    AllowedExtensions = string.Join(",", _settings.Storage.AllowedExtensions);
                    StorageProvider = _settings.Storage.StorageProvider;
                    TotalStorageQuota = _settings.Storage.TotalStorageQuota;

                    HasUnsavedChanges = false;
                }
            }
            catch (Exception ex)
            {
                await _dialogCoordinator.ShowMessageAsync(
                    this,
                    "Lỗi",
                    $"Không thể tải cài đặt: {ex.Message}",
                    MessageDialogStyle.Affirmative
                );
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SaveGeneralSettingsAsync()
        {
            try
            {
                IsLoading = true;
                var settings = new GeneralSettings
                {
                    SystemName = SystemName,
                    SystemDescription = SystemDescription,
                    SupportEmail = SupportEmail,
                    MaintenanceMode = MaintenanceMode,
                    MaintenanceMessage = MaintenanceMessage
                };

                await _settingsService.UpdateGeneralSettingsAsync(settings);
                HasUnsavedChanges = false;

                await _dialogCoordinator.ShowMessageAsync(
                    this,
                    "Thành công",
                    "Đã lưu cài đặt chung thành công!",
                    MessageDialogStyle.Affirmative
                );
            }
            catch (Exception ex)
            {
                await _dialogCoordinator.ShowMessageAsync(
                    this,
                    "Lỗi",
                    $"Không thể lưu cài đặt: {ex.Message}",
                    MessageDialogStyle.Affirmative
                );
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SaveSecuritySettingsAsync()
        {
            try
            {
                IsLoading = true;
                var settings = new SecuritySettings
                {
                    RequireEmailVerification = RequireEmailVerification,
                    SessionTimeout = SessionTimeout,
                    MaxLoginAttempts = MaxLoginAttempts,
                    LockoutDuration = LockoutDuration,
                    EnableTwoFactor = EnableTwoFactor
                };

                await _settingsService.UpdateSecuritySettingsAsync(settings);
                HasUnsavedChanges = false;

                await _dialogCoordinator.ShowMessageAsync(
                    this,
                    "Thành công",
                    "Đã lưu cài đặt bảo mật thành công!",
                    MessageDialogStyle.Affirmative
                );
            }
            catch (Exception ex)
            {
                await _dialogCoordinator.ShowMessageAsync(
                    this,
                    "Lỗi",
                    $"Không thể lưu cài đặt: {ex.Message}",
                    MessageDialogStyle.Affirmative
                );
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SaveEmailSettingsAsync()
        {
            try
            {
                IsLoading = true;
                var settings = new EmailSettings
                {
                    SmtpHost = SmtpHost,
                    SmtpPort = SmtpPort,
                    SmtpUsername = SmtpUsername,
                    SmtpPassword = SmtpPassword,
                    FromEmail = FromEmail,
                    FromName = FromName,
                    EnableSsl = EnableSsl
                };

                await _settingsService.UpdateEmailSettingsAsync(settings);
                HasUnsavedChanges = false;

                await _dialogCoordinator.ShowMessageAsync(
                    this,
                    "Thành công",
                    "Đã lưu cài đặt email thành công!",
                    MessageDialogStyle.Affirmative
                );
            }
            catch (Exception ex)
            {
                await _dialogCoordinator.ShowMessageAsync(
                    this,
                    "Lỗi",
                    $"Không thể lưu cài đặt: {ex.Message}",
                    MessageDialogStyle.Affirmative
                );
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SaveStorageSettingsAsync()
        {
            try
            {
                IsLoading = true;
                var extensions = AllowedExtensions
                    .Split(',')
                    .Select(e => e.Trim())
                    .Where(e => !string.IsNullOrEmpty(e))
                    .ToList();

                var settings = new StorageSettings
                {
                    MaxFileSize = MaxFileSize,
                    AllowedExtensions = extensions,
                    StorageProvider = StorageProvider,
                    TotalStorageQuota = TotalStorageQuota
                };

                await _settingsService.UpdateStorageSettingsAsync(settings);
                HasUnsavedChanges = false;

                await _dialogCoordinator.ShowMessageAsync(
                    this,
                    "Thành công",
                    "Đã lưu cài đặt lưu trữ thành công!",
                    MessageDialogStyle.Affirmative
                );
            }
            catch (Exception ex)
            {
                await _dialogCoordinator.ShowMessageAsync(
                    this,
                    "Lỗi",
                    $"Không thể lưu cài đặt: {ex.Message}",
                    MessageDialogStyle.Affirmative
                );
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task TestEmailConfigurationAsync()
        {
            var inputDialog = await _dialogCoordinator.ShowInputAsync(
                this,
                "Test Email",
                "Nhập địa chỉ email để test:"
            );

            if (!string.IsNullOrEmpty(inputDialog))
            {
                try
                {
                    IsLoading = true;
                    await _settingsService.TestEmailConfigurationAsync(inputDialog);

                    await _dialogCoordinator.ShowMessageAsync(
                        this,
                        "Thành công",
                        "Email test đã được gửi thành công!",
                        MessageDialogStyle.Affirmative
                    );
                }
                catch (Exception ex)
                {
                    await _dialogCoordinator.ShowMessageAsync(
                        this,
                        "Lỗi",
                        $"Không thể gửi email test: {ex.Message}",
                        MessageDialogStyle.Affirmative
                    );
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private async Task ResetToDefaultAsync()
        {
            var result = await _dialogCoordinator.ShowMessageAsync(
                this,
                "Xác nhận",
                $"Bạn có chắc chắn muốn khôi phục cài đặt {SelectedSection} về mặc định?",
                MessageDialogStyle.AffirmativeAndNegative
            );

            if (result == MessageDialogResult.Affirmative)
            {
                try
                {
                    IsLoading = true;
                    await _settingsService.ResetToDefaultAsync(SelectedSection.ToLower());
                    await LoadSettingsAsync();

                    await _dialogCoordinator.ShowMessageAsync(
                        this,
                        "Thành công",
                        "Đã khôi phục cài đặt về mặc định!",
                        MessageDialogStyle.Affirmative
                    );
                }
                catch (Exception ex)
                {
                    await _dialogCoordinator.ShowMessageAsync(
                        this,
                        "Lỗi",
                        $"Không thể khôi phục cài đặt: {ex.Message}",
                        MessageDialogStyle.Affirmative
                    );
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private async Task BackupSettingsAsync()
        {
            try
            {
                IsLoading = true;
                var data = await _settingsService.BackupSettingsAsync();

                if (data != null)
                {
                    var dialog = new Microsoft.Win32.SaveFileDialog
                    {
                        FileName = $"settings_backup_{DateTime.Now:yyyyMMdd_HHmmss}",
                        DefaultExt = ".json",
                        Filter = "JSON Files (*.json)|*.json"
                    };

                    if (dialog.ShowDialog() == true)
                    {
                        await System.IO.File.WriteAllBytesAsync(dialog.FileName, data);
                        await _dialogCoordinator.ShowMessageAsync(
                            this,
                            "Thành công",
                            "Đã sao lưu cài đặt thành công!",
                            MessageDialogStyle.Affirmative
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                await _dialogCoordinator.ShowMessageAsync(
                    this,
                    "Lỗi",
                    $"Không thể sao lưu cài đặt: {ex.Message}",
                    MessageDialogStyle.Affirmative
                );
            }
            finally
            {
                IsLoading = false;
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion
    }
}
