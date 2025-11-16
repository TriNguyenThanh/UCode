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
    public partial class UserCreateDialog : MetroWindow, INotifyPropertyChanged
    {
        private readonly AdminService _adminService;
        private string _fullName = string.Empty;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _phoneNumber = string.Empty;
        private string _address = string.Empty;
        private string _studentCode = string.Empty;
        private string _department = string.Empty;
        private ComboBoxItem? _selectedRole;
        private bool _isLoading;
        private string _validationMessage = string.Empty;
        private bool _hasValidationError;

        public UserCreateDialog(AdminService adminService)
        {
            InitializeComponent();
            _adminService = adminService;
            DataContext = this;
        }

        public string FullName
        {
            get => _fullName;
            set
            {
                _fullName = value;
                OnPropertyChanged();
                UpdateCanCreate();
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged();
                UpdateCanCreate();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
                UpdateCanCreate();
            }
        }

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
                UpdateCanCreate();
            }
        }

        public bool IsStudent => SelectedRole?.Tag?.ToString() == "Student";
        public bool IsTeacher => SelectedRole?.Tag?.ToString() == "Teacher";

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

        private bool _canCreate;
        public bool CanCreate
        {
            get => _canCreate;
            set
            {
                _canCreate = value;
                OnPropertyChanged();
            }
        }

        private void UpdateCanCreate()
        {
            CanCreate = !string.IsNullOrWhiteSpace(FullName) &&
                       !string.IsNullOrWhiteSpace(Email) &&
                       !string.IsNullOrWhiteSpace(Password) &&
                       SelectedRole != null;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                Password = passwordBox.Password;
            }
        }

        private async void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate
            HasValidationError = false;
            ValidationMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(FullName))
            {
                ShowValidationError("Full name is required");
                return;
            }

            if (string.IsNullOrWhiteSpace(Email) || !Email.Contains("@"))
            {
                ShowValidationError("Valid email is required");
                return;
            }

            if (string.IsNullOrWhiteSpace(Password) || Password.Length < 6)
            {
                ShowValidationError("Password must be at least 6 characters");
                return;
            }

            if (PasswordBox.Password != ConfirmPasswordBox.Password)
            {
                ShowValidationError("Passwords do not match");
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

                var request = new CreateUserRequest
                {
                    FullName = FullName.Trim(),
                    Email = Email.Trim(),
                    Password = Password,
                    Role = role,
                    PhoneNumber = string.IsNullOrWhiteSpace(PhoneNumber) ? null : PhoneNumber.Trim(),
                    Address = string.IsNullOrWhiteSpace(Address) ? null : Address.Trim(),
                    StudentCode = IsStudent && !string.IsNullOrWhiteSpace(StudentCode) ? StudentCode.Trim() : null,
                    Department = IsTeacher && !string.IsNullOrWhiteSpace(Department) ? Department.Trim() : null
                };

                var success = await _adminService.CreateUserAsync(request);

                if (success)
                {
                    ModernMessageBox.Show(
                        "User created successfully!",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    DialogResult = true;
                    Close();
                }
                else
                {
                    ShowValidationError("Failed to create user. Please check the information and try again.");
                }
            }
            catch (System.Exception ex)
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
