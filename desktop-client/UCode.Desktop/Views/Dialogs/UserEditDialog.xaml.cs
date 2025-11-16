using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using UCode.Desktop.Helpers;
using UCode.Desktop.Models.Admin;
using UCode.Desktop.Models.Enums;
using UCode.Desktop.Services;

namespace UCode.Desktop.Views.Dialogs
{
    public partial class UserEditDialog : MetroWindow, INotifyPropertyChanged
    {
        private readonly AdminService _adminService;
        private readonly UserManagement _user;
        private string _fullName = string.Empty;
        private string _email = string.Empty;
        private string _phoneNumber = string.Empty;
        private string _address = string.Empty;
        private string _studentCode = string.Empty;
        private string _department = string.Empty;
        private ComboBoxItem? _selectedRole;
        private bool _isActive;
        private bool _isLoading;
        private string _validationMessage = string.Empty;
        private bool _hasValidationError;

        public UserEditDialog(AdminService adminService, UserManagement user)
        {
            InitializeComponent();
            _adminService = adminService;
            _user = user;
            DataContext = this;

            // Load user data
            LoadUserData();
        }

        private void LoadUserData()
        {
            FullName = _user.FullName;
            Email = _user.Email;
            IsActive = _user.IsActive;
            PhoneNumber = _user.PhoneNumber ?? string.Empty;
            Address = _user.Address ?? string.Empty;
            StudentCode = _user.StudentCode ?? string.Empty;
            Department = _user.Department ?? string.Empty;

            // Set role in ComboBox after it's loaded
            Dispatcher.InvokeAsync(() =>
            {
                var roleComboBox = (ComboBox)this.FindName("RoleComboBox");
                if (roleComboBox != null)
                {
                    foreach (var obj in roleComboBox.Items)
                    {
                        if (obj is ComboBoxItem item && item.Tag?.ToString() == _user.Role.ToString())
                        {
                            SelectedRole = item;
                            break;
                        }
                    }
                }
            });
        }

        public string FullName
        {
            get => _fullName;
            set
            {
                _fullName = value;
                OnPropertyChanged();
                UpdateCanSave();
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged();
            }
        }

        public string UserEmail => Email;

        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                _phoneNumber = value;
                OnPropertyChanged();
            }
        }

        public string Address
        {
            get => _address;
            set
            {
                _address = value;
                OnPropertyChanged();
            }
        }

        public string StudentCode
        {
            get => _studentCode;
            set
            {
                _studentCode = value;
                OnPropertyChanged();
            }
        }

        public string Department
        {
            get => _department;
            set
            {
                _department = value;
                OnPropertyChanged();
            }
        }

        public ComboBoxItem? SelectedRole
        {
            get => _selectedRole;
            set
            {
                _selectedRole = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsStudent));
                OnPropertyChanged(nameof(IsTeacher));
                UpdateCanSave();
            }
        }

        public bool IsStudent => SelectedRole?.Tag?.ToString() == "Student";
        public bool IsTeacher => SelectedRole?.Tag?.ToString() == "Teacher";

        public new bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
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

        private bool _canSave;
        public bool CanSave
        {
            get => _canSave;
            set
            {
                _canSave = value;
                OnPropertyChanged();
            }
        }

        private void UpdateCanSave()
        {
            CanSave = !string.IsNullOrWhiteSpace(FullName) && SelectedRole != null;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate
            HasValidationError = false;
            ValidationMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(FullName))
            {
                ShowValidationError("Full name is required");
                return;
            }

            if (SelectedRole == null)
            {
                ShowValidationError("Role is required");
                return;
            }

            try
            {
                IsLoading = true;

                var roleTag = SelectedRole.Tag?.ToString() ?? "Student";
                var role = roleTag switch
                {
                    "Admin" => UserRole.Admin,
                    "Teacher" => UserRole.Teacher,
                    "Student" => UserRole.Student,
                    _ => UserRole.Student
                };

                var request = new UpdateUserRequest
                {
                    FullName = FullName.Trim(),
                    Role = role,
                    IsActive = IsActive,
                    PhoneNumber = string.IsNullOrWhiteSpace(PhoneNumber) ? null : PhoneNumber.Trim(),
                    Address = string.IsNullOrWhiteSpace(Address) ? null : Address.Trim(),
                    StudentCode = IsStudent && !string.IsNullOrWhiteSpace(StudentCode) ? StudentCode.Trim() : null,
                    Department = IsTeacher && !string.IsNullOrWhiteSpace(Department) ? Department.Trim() : null
                };

                var success = await _adminService.UpdateUserAsync(_user.Id, request);

                if (success)
                {
                    ModernMessageBox.Show(
                        "User updated successfully!",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    DialogResult = true;
                    Close();
                }
                else
                {
                    ShowValidationError("Failed to update user. Please try again.");
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
