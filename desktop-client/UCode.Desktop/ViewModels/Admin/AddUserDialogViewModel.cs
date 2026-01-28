using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Input;
using UCode.Desktop.Helpers;

namespace UCode.Desktop.ViewModels.Admin
{
    public class RoleOption
    {
        public string Value { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }

    public class AddUserDialogViewModel : INotifyPropertyChanged
    {
        private string _fullName = string.Empty;
        private string _email = string.Empty;
        private string _password = "123456";
        private RoleOption? _selectedRole;
        private string _studentCode = string.Empty;
        private string _teacherCode = string.Empty;
        private string _phoneNumber = string.Empty;
        private bool _isActive = true;
        private bool _isSubmitting = false;

        // Error messages
        private string? _fullNameError;
        private string? _emailError;
        private string? _passwordError;
        private string? _roleError;
        private string? _studentCodeError;
        private string? _teacherCodeError;

        public List<RoleOption> AvailableRoles { get; } = new()
        {
            new RoleOption { Value = "Student", DisplayName = "Sinh viên", Icon = "Account", Color = "#3B82F6" },
            new RoleOption { Value = "Teacher", DisplayName = "Giảng viên", Icon = "School", Color = "#8B5CF6" },
            new RoleOption { Value = "Admin", DisplayName = "Quản trị viên", Icon = "ShieldAccount", Color = "#EF4444" }
        };

        public string FullName
        {
            get => _fullName;
            set
            {
                if (SetProperty(ref _fullName, value))
                {
                    ValidateFullName();
                }
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                if (SetProperty(ref _email, value))
                {
                    ValidateEmail();
                }
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (SetProperty(ref _password, value))
                {
                    ValidatePassword();
                }
            }
        }

        public RoleOption? SelectedRole
        {
            get => _selectedRole;
            set
            {
                if (SetProperty(ref _selectedRole, value))
                {
                    ValidateRole();
                    OnPropertyChanged(nameof(IsStudentRole));
                    OnPropertyChanged(nameof(IsTeacherRole));

                    // Validate conditional fields
                    if (IsStudentRole)
                        ValidateStudentCode();
                    else
                        StudentCodeError = null;

                    if (IsTeacherRole)
                        ValidateTeacherCode();
                    else
                        TeacherCodeError = null;
                }
            }
        }

        public string StudentCode
        {
            get => _studentCode;
            set
            {
                if (SetProperty(ref _studentCode, value))
                {
                    ValidateStudentCode();
                }
            }
        }

        public string TeacherCode
        {
            get => _teacherCode;
            set
            {
                if (SetProperty(ref _teacherCode, value))
                {
                    ValidateTeacherCode();
                }
            }
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set => SetProperty(ref _phoneNumber, value);
        }

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        public bool IsSubmitting
        {
            get => _isSubmitting;
            set => SetProperty(ref _isSubmitting, value);
        }

        public bool IsStudentRole => SelectedRole?.Value == "Student";
        public bool IsTeacherRole => SelectedRole?.Value == "Teacher";

        // Error properties
        public string? FullNameError
        {
            get => _fullNameError;
            set => SetProperty(ref _fullNameError, value);
        }

        public string? EmailError
        {
            get => _emailError;
            set => SetProperty(ref _emailError, value);
        }

        public string? PasswordError
        {
            get => _passwordError;
            set => SetProperty(ref _passwordError, value);
        }

        public string? RoleError
        {
            get => _roleError;
            set => SetProperty(ref _roleError, value);
        }

        public string? StudentCodeError
        {
            get => _studentCodeError;
            set => SetProperty(ref _studentCodeError, value);
        }

        public string? TeacherCodeError
        {
            get => _teacherCodeError;
            set => SetProperty(ref _teacherCodeError, value);
        }

        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler<bool>? DialogClosed;

        public AddUserDialogViewModel()
        {
            ConfirmCommand = new RelayCommand(_ => OnConfirm(), _ => CanConfirm());
            CancelCommand = new RelayCommand(_ => OnCancel());

            // Set default role
            SelectedRole = AvailableRoles.First();
        }

        private void ValidateFullName()
        {
            if (string.IsNullOrWhiteSpace(FullName))
            {
                FullNameError = "Họ tên không được để trống";
            }
            else
            {
                FullNameError = null;
            }
        }

        private void ValidateEmail()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                EmailError = "Email không được để trống";
            }
            else if (!Regex.IsMatch(Email, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
            {
                EmailError = "Email không đúng định dạng";
            }
            else
            {
                EmailError = null;
            }
        }

        private void ValidatePassword()
        {
            if (string.IsNullOrEmpty(Password))
            {
                PasswordError = "Mật khẩu không được để trống";
            }
            else if (Password.Length < 6)
            {
                PasswordError = "Mật khẩu phải có ít nhất 6 ký tự";
            }
            else
            {
                PasswordError = null;
            }
        }

        private void ValidateRole()
        {
            if (SelectedRole == null)
            {
                RoleError = "Vui lòng chọn vai trò";
            }
            else
            {
                RoleError = null;
            }
        }

        private void ValidateStudentCode()
        {
            if (IsStudentRole && string.IsNullOrWhiteSpace(StudentCode))
            {
                StudentCodeError = "Mã sinh viên không được để trống";
            }
            else
            {
                StudentCodeError = null;
            }
        }

        private void ValidateTeacherCode()
        {
            if (IsTeacherRole && string.IsNullOrWhiteSpace(TeacherCode))
            {
                TeacherCodeError = "Mã giảng viên không được để trống";
            }
            else
            {
                TeacherCodeError = null;
            }
        }

        private bool ValidateAll()
        {
            ValidateFullName();
            ValidateEmail();
            ValidatePassword();
            ValidateRole();

            if (IsStudentRole)
                ValidateStudentCode();

            if (IsTeacherRole)
                ValidateTeacherCode();

            return string.IsNullOrEmpty(FullNameError) &&
                   string.IsNullOrEmpty(EmailError) &&
                   string.IsNullOrEmpty(PasswordError) &&
                   string.IsNullOrEmpty(RoleError) &&
                   string.IsNullOrEmpty(StudentCodeError) &&
                   string.IsNullOrEmpty(TeacherCodeError);
        }

        private bool CanConfirm()
        {
            return !IsSubmitting &&
                   !string.IsNullOrWhiteSpace(FullName) &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrEmpty(Password) &&
                   SelectedRole != null;
        }

        private void OnConfirm()
        {
            if (ValidateAll())
            {
                DialogClosed?.Invoke(this, true);
            }
        }

        private void OnCancel()
        {
            DialogClosed?.Invoke(this, false);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
