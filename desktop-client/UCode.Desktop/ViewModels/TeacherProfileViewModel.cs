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
    public class TeacherProfileViewModel : ViewModelBase
    {
        private readonly UserService _userService;
        private readonly TeacherService _teacherService;
        private readonly AuthService _authService;
        private User _teacher;

        private string _fullName;
        private string _email;
        private string _teacherCode;
        private string _department;
        private string _title;
        private string _phone;
        private string _initials;
        private bool _isLoading;
        private bool _isEditMode;

        public TeacherProfileViewModel(UserService userService, TeacherService teacherService, AuthService authService)
        {
            _userService = userService;
            _teacherService = teacherService;
            _authService = authService;

            EditProfileCommand = new RelayCommand(_ => ToggleEditMode(), _ => !IsLoading);
            SaveProfileCommand = new RelayCommand(async _ => await SaveProfileAsync(), _ => IsEditMode && !IsLoading);
        }

        #region Properties

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        public string FullName
        {
            get => _fullName;
            set
            {
                SetProperty(ref _fullName, value);
                UpdateInitials();
            }
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string TeacherCode
        {
            get => _teacherCode;
            set => SetProperty(ref _teacherCode, value);
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

        public string Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }

        public string Initials
        {
            get => _initials;
            set => SetProperty(ref _initials, value);
        }

        #endregion

        #region Commands

        public ICommand EditProfileCommand { get; }
        public ICommand SaveProfileCommand { get; }

        #endregion

        public async System.Threading.Tasks.Task LoadTeacherDataAsync()
        {
            IsLoading = true;
            try
            {
                var response = await _teacherService.GetMyProfileAsync();
                if (response?.Success == true && response.Data != null)
                {
                    _teacher = response.Data;
                    
                    FullName = _teacher.FullName;
                    Email = _teacher.Email;
                    TeacherCode = _teacher.TeacherCode;
                    Department = _teacher.Department;
                    Title = _teacher.Title;
                    Phone = _teacher.Phone;
                    
                    UpdateInitials();
                }
                else
                {
                    await GetMetroWindow()?.ShowMessageAsync("Lỗi", $"Không thể tải thông tin giảng viên: {response?.Message}");
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

        private void UpdateInitials()
        {
            if (string.IsNullOrWhiteSpace(FullName))
            {
                Initials = "?";
                return;
            }

            var parts = FullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                // Get first letter of first name and last name
                Initials = $"{parts[0][0]}{parts[parts.Length - 1][0]}".ToUpper();
            }
            else
            {
                // Just get first letter
                Initials = parts[0][0].ToString().ToUpper();
            }
        }

        private void ToggleEditMode()
        {
            IsEditMode = !IsEditMode;
        }

        private async System.Threading.Tasks.Task SaveProfileAsync()
        {
            if (string.IsNullOrWhiteSpace(FullName))
            {
                await GetMetroWindow()?.ShowMessageAsync("Lỗi", "Vui lòng nhập họ và tên");
                return;
            }

            IsLoading = true;
            try
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
                
                if (response?.Success == true)
                {
                    await GetMetroWindow()?.ShowMessageAsync("Thành công", "Cập nhật hồ sơ thành công");
                    IsEditMode = false;
                    
                    // Reload data
                    await LoadTeacherDataAsync();
                }
                else
                {
                    await GetMetroWindow()?.ShowMessageAsync("Lỗi", $"Cập nhật thất bại: {response?.Message}");
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
