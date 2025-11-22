using System;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using UCode.Desktop.Helpers;
using UCode.Desktop.Models;
using UCode.Desktop.Services;

namespace UCode.Desktop.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly UserService _userService;
        private readonly StudentService _studentService;
        private readonly TeacherService _teacherService;
        private readonly AuthService _authService;
        private User _currentUser;

        // Profile fields
        private string _fullName;
        private string _email;
        private string _phone;
        private string _major;
        private int? _classYear;
        private string _department;
        private string _title;

        // Password fields
        private string _currentPassword;
        private string _newPassword;
        private string _confirmPassword;
        
        private string _currentPasswordError;
        private string _newPasswordError;
        private string _confirmPasswordError;
        private bool _isPasswordValid;

        private bool _isLoading;
        private bool _isStudent;
        private bool _isTeacher;

        public SettingsViewModel(UserService userService, StudentService studentService, TeacherService teacherService, AuthService authService)
        {
            _userService = userService;
            _studentService = studentService;
            _teacherService = teacherService;
            _authService = authService;

            UpdateProfileCommand = new RelayCommand(async _ => await UpdateProfileAsync(), _ => !IsLoading);
            ChangePasswordCommand = new RelayCommand(async _ => await ChangePasswordAsync(), _ => !IsLoading);
        }

        #region Properties

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool IsStudent
        {
            get => _isStudent;
            set => SetProperty(ref _isStudent, value);
        }

        public bool IsTeacher
        {
            get => _isTeacher;
            set => SetProperty(ref _isTeacher, value);
        }

        public string FullName
        {
            get => _fullName;
            set => SetProperty(ref _fullName, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }

        public string Major
        {
            get => _major;
            set => SetProperty(ref _major, value);
        }

        public int? ClassYear
        {
            get => _classYear;
            set => SetProperty(ref _classYear, value);
        }

        public string Department
        {
            get => _department;
            set => SetProperty(ref _department, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string CurrentPassword
        {
            get => _currentPassword;
            set 
            {
                SetProperty(ref _currentPassword, value);
                ValidatePassword();
            }
        }

        public string NewPassword
        {
            get => _newPassword;
            set 
            {
                SetProperty(ref _newPassword, value);
                ValidatePassword();
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set 
            {
                SetProperty(ref _confirmPassword, value);
                ValidatePassword();
            }
        }

        public string CurrentPasswordError
        {
            get => _currentPasswordError;
            set => SetProperty(ref _currentPasswordError, value);
        }

        public string NewPasswordError
        {
            get => _newPasswordError;
            set => SetProperty(ref _newPasswordError, value);
        }

        public string ConfirmPasswordError
        {
            get => _confirmPasswordError;
            set => SetProperty(ref _confirmPasswordError, value);
        }

        public bool IsPasswordValid
        {
            get => _isPasswordValid;
            set => SetProperty(ref _isPasswordValid, value);
        }

        #endregion

        #region Commands

        public ICommand UpdateProfileCommand { get; }
        public ICommand ChangePasswordCommand { get; }

        #endregion

        public async System.Threading.Tasks.Task LoadUserDataAsync()
        {
            IsLoading = true;
            try
            {
                var user = _authService.CurrentUser;
                if (user == null) return;

                _currentUser = user; // Keep reference for UserId
                IsStudent = user.Role == UserRole.Student;
                IsTeacher = user.Role == UserRole.Teacher || user.Role == UserRole.Admin;

                if (IsStudent)
                {
                    var response = await _studentService.GetMyProfileAsync();
                    if (response?.Success == true && response.Data != null)
                    {
                        var student = response.Data;
                        FullName = student.FullName;
                        Email = student.Email;
                        Phone = student.Phone;
                        Major = student.Major;
                        ClassYear = student.ClassYear;
                    }
                }
                else if (IsTeacher)
                {
                    var response = await _teacherService.GetMyProfileAsync();
                    if (response?.Success == true && response.Data != null)
                    {
                        var teacher = response.Data;
                        FullName = teacher.FullName;
                        Email = teacher.Email;
                        Phone = teacher.Phone;
                        Department = teacher.Department;
                        Title = teacher.Title;
                    }
                }
            }
            catch (Exception ex)
            {
                await GetMetroWindow()?.ShowMessageAsync("Lỗi", $"Đã xảy ra lỗi: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async System.Threading.Tasks.Task UpdateProfileAsync()
        {
            if (string.IsNullOrWhiteSpace(FullName))
            {
                await GetMetroWindow()?.ShowMessageAsync("Lỗi", "Vui lòng nhập họ và tên");
                return;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                await GetMetroWindow()?.ShowMessageAsync("Lỗi", "Vui lòng nhập email");
                return;
            }

            IsLoading = true;
            try
            {
                bool success = false;
                string message = string.Empty;

                if (IsStudent)
                {
                    var request = new UpdateStudentRequest
                    {
                        FullName = FullName,
                        Email = Email,
                        Phone = Phone,
                        Major = Major,
                        ClassYear = ClassYear
                    };
                    var response = await _studentService.UpdateMyProfileAsync(request);
                    success = response?.Success == true;
                    message = response?.Message;
                }
                else if (IsTeacher)
                {
                    var request = new UpdateTeacherRequest
                    {
                        FullName = FullName,
                        Email = Email,
                        Phone = Phone,
                        Department = Department,
                        Title = Title
                    };
                    var response = await _teacherService.UpdateMyProfileAsync(request);
                    success = response?.Success == true;
                    message = response?.Message;
                }

                if (success)
                {
                    await GetMetroWindow()?.ShowMessageAsync("Thành công", "Cập nhật thông tin cá nhân thành công");
                    await LoadUserDataAsync();
                }
                else
                {
                    await GetMetroWindow()?.ShowMessageAsync("Lỗi", $"Cập nhật thất bại: {message}");
                }
            }
            catch (Exception ex)
            {
                await GetMetroWindow()?.ShowMessageAsync("Lỗi", $"Đã xảy ra lỗi: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ValidatePassword()
        {
            bool isValid = true;

            // Validate Current Password
            if (string.IsNullOrEmpty(CurrentPassword))
            {
                CurrentPasswordError = null; // Don't show error initially or if empty
                isValid = false;
            }
            else
            {
                CurrentPasswordError = null;
            }

            // Validate New Password
            if (string.IsNullOrEmpty(NewPassword))
            {
                NewPasswordError = null;
                isValid = false;
            }
            else if (NewPassword.Length < 6)
            {
                NewPasswordError = "Mật khẩu mới phải có ít nhất 6 ký tự";
                isValid = false;
            }
            else
            {
                NewPasswordError = null;
            }

            // Validate Confirm Password
            if (string.IsNullOrEmpty(ConfirmPassword))
            {
                ConfirmPasswordError = null;
                isValid = false;
            }
            else if (ConfirmPassword != NewPassword)
            {
                ConfirmPasswordError = "Mật khẩu xác nhận không khớp";
                isValid = false;
            }
            else
            {
                ConfirmPasswordError = null;
            }

            // Additional check to ensure all fields are filled for the button to be active
            if (string.IsNullOrEmpty(CurrentPassword) || string.IsNullOrEmpty(NewPassword) || string.IsNullOrEmpty(ConfirmPassword))
            {
                isValid = false;
            }

            IsPasswordValid = isValid;
        }

        private async System.Threading.Tasks.Task ChangePasswordAsync()
        {
            ValidatePassword();
            if (!IsPasswordValid) return;

            IsLoading = true;
            try
            {
                var request = new ChangePasswordRequest
                {
                    UserId = _currentUser.UserId,
                    OldPassword = CurrentPassword,
                    NewPassword = NewPassword,
                    ConfirmPassword = ConfirmPassword
                };

                var response = await _userService.ChangePasswordAsync(request);
                
                if (response?.Success == true)
                {
                    await GetMetroWindow()?.ShowMessageAsync("Thành công", "Đổi mật khẩu thành công");
                    
                    // Clear password fields
                    CurrentPassword = string.Empty;
                    NewPassword = string.Empty;
                    ConfirmPassword = string.Empty;
                    
                    // Reset errors
                    CurrentPasswordError = null;
                    NewPasswordError = null;
                    ConfirmPasswordError = null;
                    IsPasswordValid = false;
                }
                else
                {
                    await GetMetroWindow()?.ShowMessageAsync("Lỗi", $"Đổi mật khẩu thất bại: {response?.Message}");
                }
            }
            catch (Exception ex)
            {
                await GetMetroWindow()?.ShowMessageAsync("Lỗi", $"Đã xảy ra lỗi: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
