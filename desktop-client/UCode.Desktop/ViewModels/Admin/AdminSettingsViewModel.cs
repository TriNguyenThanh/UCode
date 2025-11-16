using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UCode.Desktop.Helpers;
using UCode.Desktop.Models.Admin;
using UCode.Desktop.Services;

namespace UCode.Desktop.ViewModels.Admin
{
    public class AdminSettingsViewModel : INotifyPropertyChanged
    {
        private readonly AdminService _adminService;
        private SystemSettings _settings = new();
        private bool _isLoading;
        private bool _hasChanges;

        public AdminSettingsViewModel(AdminService adminService)
        {
            _adminService = adminService;

            SaveCommand = new RelayCommand(async _ => await SaveSettingsAsync(), _ => HasChanges);
            ResetCommand = new RelayCommand(async _ => await LoadSettingsAsync());
            TestEmailCommand = new RelayCommand(_ => TestEmailSettings());
        }

        #region Properties

        public SystemSettings Settings
        {
            get => _settings;
            set
            {
                _settings = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public bool HasChanges
        {
            get => _hasChanges;
            set
            {
                _hasChanges = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        public ICommand SaveCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand TestEmailCommand { get; }

        #endregion

        #region Methods

        public async Task LoadSettingsAsync()
        {
            try
            {
                IsLoading = true;
                var settings = await _adminService.GetSystemSettingsAsync();
                
                if (settings != null)
                {
                    Settings = settings;
                    HasChanges = false;
                }
            }
            catch (Exception ex)
            {
                ModernMessageBox.Show(
                    $"Failed to load settings: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SaveSettingsAsync()
        {
            try
            {
                IsLoading = true;
                var success = await _adminService.UpdateSystemSettingsAsync(Settings);
                
                if (success)
                {
                    HasChanges = false;
                    ModernMessageBox.Show(
                        "Settings saved successfully",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    ModernMessageBox.Show(
                        "Failed to save settings",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                ModernMessageBox.Show(
                    $"Error saving settings: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void TestEmailSettings()
        {
            ModernMessageBox.Show(
                "Test email functionality will be implemented.\n" +
                $"SMTP Server: {Settings.SmtpServer}:{Settings.SmtpPort}\n" +
                $"Username: {Settings.SmtpUsername}\n" +
                $"SSL: {Settings.EnableSsl}",
                "Email Test",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        public void MarkAsChanged()
        {
            HasChanges = true;
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
