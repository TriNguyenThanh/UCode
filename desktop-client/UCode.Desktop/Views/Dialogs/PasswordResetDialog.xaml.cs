using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using UCode.Desktop.Helpers;
using UCode.Desktop.Models.Admin;
using UCode.Desktop.Services;

namespace UCode.Desktop.Views.Dialogs
{
    public partial class PasswordResetDialog : MetroWindow, INotifyPropertyChanged
    {
        private readonly AdminService _adminService;
        private readonly UserManagement _user;
        private string _password = string.Empty;
        private bool _isLoading;
        private string _validationMessage = string.Empty;
        private bool _hasValidationError;

        public PasswordResetDialog(AdminService adminService, UserManagement user)
        {
            InitializeComponent();
            _adminService = adminService;
            _user = user;
            DataContext = this;
        }

        public string UserInfo => $"{_user.FullName} ({_user.Email})";

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
                UpdateCanReset();
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

        public string ValidationMessage
        {
            get => _validationMessage;
            set
            {
                _validationMessage = value;
                OnPropertyChanged();
            }
        }

        public bool HasValidationError
        {
            get => _hasValidationError;
            set
            {
                _hasValidationError = value;
                OnPropertyChanged();
            }
        }

        private bool _canReset;
        public bool CanReset
        {
            get => _canReset;
            set
            {
                _canReset = value;
                OnPropertyChanged();
            }
        }

        private void UpdateCanReset()
        {
            CanReset = !string.IsNullOrWhiteSpace(Password) && Password.Length >= 6;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                Password = passwordBox.Password;
            }
        }

        private async void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate
            HasValidationError = false;
            ValidationMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(NewPasswordBox.Password))
            {
                ShowValidationError("Password is required");
                return;
            }

            if (NewPasswordBox.Password.Length < 6)
            {
                ShowValidationError("Password must be at least 6 characters");
                return;
            }

            if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
            {
                ShowValidationError("Passwords do not match");
                return;
            }

            var result = ModernMessageBox.Show(
                $"Are you sure you want to reset the password for '{_user.FullName}'?",
                "Confirm Password Reset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                IsLoading = true;

                var success = await _adminService.ResetUserPasswordAsync(_user.Id, NewPasswordBox.Password);

                if (success)
                {
                    ModernMessageBox.Show(
                        "Password reset successfully!",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    DialogResult = true;
                    Close();
                }
                else
                {
                    ShowValidationError("Failed to reset password. Please try again.");
                }
            }
            catch (Exception ex)
            {
                ShowValidationError($"Error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ShowValidationError(string message)
        {
            ValidationMessage = message;
            HasValidationError = true;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
