using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using UCode.Desktop.Helpers;
using UCode.Desktop.Models;
using UCode.Desktop.Models.Admin;
using UCode.Desktop.Services;
using UCode.Desktop.Services.Admin;

namespace UCode.Desktop.ViewModels.Admin
{
    /// <summary>
    /// Item wrapper cho User trong UI
    /// </summary>
    public class UserItem : INotifyPropertyChanged
    {
        private bool _isSelected = false;
        private bool _isActive;

        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(StatusDisplay));
                }
            }
        }
        
        public string? StudentCode { get; set; }
        public string? Major { get; set; }
        public int? ClassYear { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public string StatusDisplay => IsActive ? "Hoạt động" : "Không hoạt động";
        public string LastLoginDisplay => LastLoginAt?.ToString("dd/MM/yyyy HH:mm") ?? "Chưa đăng nhập";

        public UserItem(AdminUserResponse user)
        {
            UserId = user.UserId;
            Username = user.Email.Split('@')[0]; // Extract username from email
            Email = user.Email;
            FullName = user.FullName;
            Role = user.Role;
            IsActive = user.IsActive; // Direct mapping from AdminUserResponse
            StudentCode = user.StudentCode;
            Major = null; // Not available in AdminUserResponse
            ClassYear = null; // Not available in AdminUserResponse
            CreatedAt = user.CreatedAt;
            LastLoginAt = null; // Not available in AdminUserResponse
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// ViewModel cho Admin User Management
    /// </summary>
    public class AdminUsersViewModel : INotifyPropertyChanged
    {
        private readonly AdminUserService _userService;
        private readonly NavigationService _navigationService;
        private readonly IDialogCoordinator _dialogCoordinator;

        private bool _isLoading;
        private string _searchTerm = string.Empty;
        private string _selectedRole = "All";
        private string? _selectedStatus;
        private int _currentPage = 1;
        private int _totalPages = 1;
        private int _totalUsers = 0;
        private const int PageSize = 50;

        public ObservableCollection<UserItem> Users { get; } = new();
        public List<string> Roles { get; } = new() { "All", "Student", "Teacher", "Admin" };
        public List<string> Statuses { get; } = new() { "All", "Active", "Inactive" };

        #region Properties

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                if (SetProperty(ref _searchTerm, value))
                {
                    // _ = SearchUsersAsync();
                }
            }
        }

        public string SelectedRole
        {
            get => _selectedRole;
            set
            {
                if (SetProperty(ref _selectedRole, value))
                {
                    _ = LoadUsersAsync();
                }
            }
        }

        public string? SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                if (SetProperty(ref _selectedStatus, value))
                {
                    _ = LoadUsersAsync();
                }
            }
        }

        public int CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }

        public int TotalPages
        {
            get => _totalPages;
            set => SetProperty(ref _totalPages, value);
        }

        public int TotalUsers
        {
            get => _totalUsers;
            set => SetProperty(ref _totalUsers, value);
        }

        public bool HasSelection => Users.Any(u => u.IsSelected);
        public int SelectedCount => Users.Count(u => u.IsSelected);

        #endregion

        #region Commands

        public ICommand LoadUsersCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand CreateUserCommand { get; }
        public ICommand EditUserCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand ViewDetailsCommand { get; }
        public ICommand ResetPasswordCommand { get; }
        public ICommand ToggleActiveCommand { get; }
        public ICommand BulkDeleteCommand { get; }
        public ICommand BulkActivateCommand { get; }
        public ICommand BulkDeactivateCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand RefreshCommand { get; }

        #endregion

        public AdminUsersViewModel(
            AdminUserService userService,
            NavigationService navigationService,
            IDialogCoordinator dialogCoordinator)
        {
            _userService = userService;
            _navigationService = navigationService;
            _dialogCoordinator = dialogCoordinator;

            LoadUsersCommand = new RelayCommand(async _ => await LoadUsersAsync());
            SearchCommand = new RelayCommand(async _ => await SearchUsersAsync());
            CreateUserCommand = new RelayCommand(async _ => await CreateUserAsync());
            EditUserCommand = new RelayCommand<UserItem>(async user => await EditUserAsync(user));
            DeleteUserCommand = new RelayCommand<UserItem>(async user => await DeleteUserAsync(user));
            ViewDetailsCommand = new RelayCommand<UserItem>(async user => await ViewDetailsAsync(user));
            ResetPasswordCommand = new RelayCommand<UserItem>(async user => await ResetPasswordAsync(user));
            ToggleActiveCommand = new RelayCommand<UserItem>(async user => await ToggleActiveAsync(user));
            BulkDeleteCommand = new RelayCommand(async _ => await BulkDeleteAsync(), (Predicate<object>)(_ => HasSelection));
            BulkActivateCommand = new RelayCommand(async _ => await BulkActivateAsync(), (Predicate<object>)(_ => HasSelection));
            BulkDeactivateCommand = new RelayCommand(async _ => await BulkDeactivateAsync(), (Predicate<object>)(_ => HasSelection));
            ExportCommand = new RelayCommand(async _ => await ExportUsersAsync());
            NextPageCommand = new RelayCommand(async _ => await NextPageAsync(), (Predicate<object>)(_ => CurrentPage < TotalPages));
            PreviousPageCommand = new RelayCommand(async _ => await PreviousPageAsync(), (Predicate<object>)(_ => CurrentPage > 1));
            RefreshCommand = new RelayCommand(async _ => await RefreshAsync());
        }

        public async Task InitializeAsync()
        {
            await LoadUsersAsync();
        }

        public async Task RefreshAsync()
        {
            //clear bộ search và lọc
            SearchTerm = string.Empty;
            SelectedRole = "All";
            SelectedStatus = "All";
            
            await LoadUsersAsync();

            OnPropertyChanged(nameof(SearchTerm));
            OnPropertyChanged(nameof(SelectedRole));
            OnPropertyChanged(nameof(SelectedStatus));

        }

        public async Task LoadUsersAsync()
        {
            IsLoading = true;

            try
            {
                var role = SelectedRole == "All" ? null : SelectedRole;
                bool? isActive = SelectedStatus switch
                {
                    "Active" => true,
                    "Inactive" => false,
                    _ => null
                };

                var result = await _userService.GetAllUsersAsync(
                    role: role,
                    isActive: isActive,
                    searchTerm: string.IsNullOrWhiteSpace(SearchTerm) ? null : SearchTerm,
                    pageNumber: CurrentPage,
                    pageSize: PageSize
                );

                if (result != null)
                {
                    Users.Clear();
                    foreach (var user in result.Items)
                    {
                        var userItem = new UserItem(user);
                        userItem.PropertyChanged += OnUserItemPropertyChanged;
                        System.Diagnostics.Debug.WriteLine($"User: {user.FullName}, Email: {user.Email}, IsActive from API: {user.IsActive}, IsActive mapped: {userItem.IsActive}");
                        Users.Add(userItem);
                    }

                    TotalPages = result.TotalPages;
                    TotalUsers = result.TotalCount;
                }
            }
            catch (Exception ex)
            {
                await _dialogCoordinator.ShowMessageAsync(
                    this,
                    "Lỗi",
                    $"Không thể tải danh sách người dùng: {ex.Message}",
                    MessageDialogStyle.Affirmative
                );
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchUsersAsync()
        {
            CurrentPage = 1;
            await LoadUsersAsync();
        }

        private async Task CreateUserAsync()
        {
            var dialogViewModel = new AddUserDialogViewModel();
            bool? result = null;

            dialogViewModel.DialogClosed += (sender, confirmed) =>
            {
                result = confirmed;
            };

            var view = new UCode.Desktop.Views.Admin.AddUserDialog
            {
                DataContext = dialogViewModel
            };

            var customDialog = new CustomDialog
            {
                Content = view
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);

            // Wait for dialog to close
            while (result == null)
            {
                await Task.Delay(100);
            }

            await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

            if (result == true)
            {
                try
                {
                    dialogViewModel.IsSubmitting = true;

                    // Xác định username dựa trên role
                    string username;
                    if (dialogViewModel.IsStudentRole)
                    {
                        username = dialogViewModel.StudentCode?.Trim() ?? string.Empty;
                    }
                    else if (dialogViewModel.IsTeacherRole)
                    {
                        username = dialogViewModel.TeacherCode?.Trim() ?? string.Empty;
                    }
                    else
                    {
                        // Admin: lấy phần trước @ của email làm username
                        username = dialogViewModel.Email.Split('@')[0];
                    }

                    var request = new CreateUserByAdminRequest
                    {
                        Username = username,
                        FullName = dialogViewModel.FullName.Trim(),
                        Email = dialogViewModel.Email.Trim(),
                        Password = dialogViewModel.Password,
                        Role = dialogViewModel.SelectedRole?.Value ?? "Student",
                        IsActive = dialogViewModel.IsActive,
                        StudentCode = dialogViewModel.IsStudentRole ? dialogViewModel.StudentCode?.Trim() : null,
                        TeacherCode = dialogViewModel.IsTeacherRole ? dialogViewModel.TeacherCode?.Trim() : null,
                        PhoneNumber = dialogViewModel.IsTeacherRole ? dialogViewModel.PhoneNumber?.Trim() : null,
                        Phone = dialogViewModel.IsTeacherRole ? dialogViewModel.PhoneNumber?.Trim() : null
                    };

                    var createdUser = await _userService.CreateUserAsync(request);

                    if (createdUser != null)
                    {
                        await _dialogCoordinator.ShowMessageAsync(
                            this,
                            "Thành công",
                            $"Đã thêm người dùng '{dialogViewModel.FullName}' thành công!",
                            MessageDialogStyle.Affirmative
                        );

                        // Refresh list
                        await LoadUsersAsync();
                    }
                }
                catch (Exception ex)
                {
                    await _dialogCoordinator.ShowMessageAsync(
                        this,
                        "Lỗi",
                        $"Không thể tạo người dùng: {ex.Message}",
                        MessageDialogStyle.Affirmative
                    );
                }
                finally
                {
                    dialogViewModel.IsSubmitting = false;
                }
            }
        }

        private async Task EditUserAsync(UserItem? user)
        {
            if (user == null) return;

            // TODO: Show EditUserDialog
            await _dialogCoordinator.ShowMessageAsync(
                this,
                "Chức năng",
                $"Tính năng chỉnh sửa người dùng {user.FullName} đang được phát triển.",
                MessageDialogStyle.Affirmative
            );
        }

        private async Task DeleteUserAsync(UserItem? user)
        {
            if (user == null) return;

            var result = await _dialogCoordinator.ShowMessageAsync(
                this,
                "Xác nhận xóa",
                $"Bạn có chắc muốn xóa người dùng {user.FullName}?\n\nThao tác này không thể hoàn tác!",
                MessageDialogStyle.AffirmativeAndNegative
            );

            if (result != MessageDialogResult.Affirmative) return;

            IsLoading = true;

            try
            {
                var success = await _userService.DeleteUserAsync(user.UserId);
                if (success)
                {
                    Users.Remove(user);
                    TotalUsers--;
                    
                    await _dialogCoordinator.ShowMessageAsync(
                        this,
                        "Thành công",
                        "Đã xóa người dùng.",
                        MessageDialogStyle.Affirmative
                    );
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ViewDetailsAsync(UserItem? user)
        {
            if (user == null) return;

            IsLoading = true;

            try
            {
                var details = await _userService.GetUserDetailAsync(user.UserId);
                
                if (details != null)
                {
                    var message = $"Username: {details.Username}\n" +
                                $"Email: {details.Email}\n" +
                                $"Role: {details.Role}\n" +
                                $"Status: {(details.IsActive ? "Hoạt động" : "Không hoạt động")}\n" +
                                $"Classes Enrolled: {details.ClassesEnrolled}\n" +
                                $"Classes Teaching: {details.ClassesTeaching}\n" +
                                $"Submissions: {details.SubmissionsCount}\n" +
                                $"Problems Created: {details.ProblemsCreated}\n" +
                                $"Created: {details.CreatedAt:dd/MM/yyyy}\n" +
                                $"Last Login: {details.LastLoginAt?.ToString("dd/MM/yyyy HH:mm") ?? "Chưa đăng nhập"}";

                    await _dialogCoordinator.ShowMessageAsync(
                        this,
                        $"Chi tiết: {details.FullName}",
                        message,
                        MessageDialogStyle.Affirmative
                    );
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ResetPasswordAsync(UserItem? user)
        {
            if (user == null) return;

            var newPassword = await _dialogCoordinator.ShowInputAsync(
                this,
                "Đặt lại mật khẩu",
                $"Nhập mật khẩu mới cho {user.FullName}:"
            );

            if (string.IsNullOrWhiteSpace(newPassword)) return;

            IsLoading = true;

            try
            {
                var success = await _userService.ResetPasswordAsync(user.UserId, newPassword);
                if (success)
                {
                    await _dialogCoordinator.ShowMessageAsync(
                        this,
                        "Thành công",
                        "Đã đặt lại mật khẩu.",
                        MessageDialogStyle.Affirmative
                    );
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ToggleActiveAsync(UserItem? user)
        {
            if (user == null) return;

            var action = user.IsActive ? "deactivate" : "activate";
            var actionText = user.IsActive ? "vô hiệu hóa" : "kích hoạt";

            var result = await _dialogCoordinator.ShowMessageAsync(
                this,
                "Xác nhận",
                $"Bạn có chắc muốn {actionText} tài khoản {user.FullName}?",
                MessageDialogStyle.AffirmativeAndNegative
            );

            if (result != MessageDialogResult.Affirmative) return;

            IsLoading = true;

            try
            {
                var bulkResult = await _userService.BulkActionAsync(
                    action,
                    new List<string> { user.UserId }
                );

                if (bulkResult != null && bulkResult.SuccessCount > 0)
                {
                    await _dialogCoordinator.ShowMessageAsync(
                        this,
                        "Thành công",
                        $"Đã {actionText} tài khoản.",
                        MessageDialogStyle.Affirmative
                    );
                    
                    // Reload data from server to ensure consistency
                    await LoadUsersAsync();
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task BulkDeleteAsync()
        {
            var selectedUsers = Users.Where(u => u.IsSelected).ToList();
            if (!selectedUsers.Any()) return;

            var result = await _dialogCoordinator.ShowMessageAsync(
                this,
                "Xác nhận xóa",
                $"Bạn có chắc muốn xóa {selectedUsers.Count} người dùng đã chọn?\n\nThao tác này không thể hoàn tác!",
                MessageDialogStyle.AffirmativeAndNegative
            );

            if (result != MessageDialogResult.Affirmative) return;

            IsLoading = true;

            try
            {
                var userIds = selectedUsers.Select(u => u.UserId).ToList();
                var bulkResult = await _userService.BulkActionAsync("delete", userIds);

                if (bulkResult != null)
                {
                    await _dialogCoordinator.ShowMessageAsync(
                        this,
                        "Kết quả",
                        $"Đã xóa: {bulkResult.SuccessCount}\nThất bại: {bulkResult.FailedCount}",
                        MessageDialogStyle.Affirmative
                    );

                    await LoadUsersAsync();
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task BulkActivateAsync()
        {
            var selectedUsers = Users.Where(u => u.IsSelected).ToList();
            if (!selectedUsers.Any()) return;

            IsLoading = true;

            try
            {
                var userIds = selectedUsers.Select(u => u.UserId).ToList();
                var bulkResult = await _userService.BulkActionAsync("activate", userIds);

                if (bulkResult != null)
                {
                    await _dialogCoordinator.ShowMessageAsync(
                        this,
                        "Kết quả",
                        $"Đã kích hoạt: {bulkResult.SuccessCount}\nThất bại: {bulkResult.FailedCount}",
                        MessageDialogStyle.Affirmative
                    );

                    await LoadUsersAsync();
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task BulkDeactivateAsync()
        {
            var selectedUsers = Users.Where(u => u.IsSelected).ToList();
            if (!selectedUsers.Any()) return;

            IsLoading = true;

            try
            {
                var userIds = selectedUsers.Select(u => u.UserId).ToList();
                var bulkResult = await _userService.BulkActionAsync("deactivate", userIds);

                if (bulkResult != null)
                {
                    await _dialogCoordinator.ShowMessageAsync(
                        this,
                        "Kết quả",
                        $"Đã vô hiệu hóa: {bulkResult.SuccessCount}\nThất bại: {bulkResult.FailedCount}",
                        MessageDialogStyle.Affirmative
                    );

                    await LoadUsersAsync();
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExportUsersAsync()
        {
            await _dialogCoordinator.ShowMessageAsync(
                this,
                "Chức năng",
                "Tính năng xuất Excel đang được phát triển.",
                MessageDialogStyle.Affirmative
            );
        }

        private async Task NextPageAsync()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                await LoadUsersAsync();
            }
        }

        private async Task PreviousPageAsync()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                await LoadUsersAsync();
            }
        }

        private void OnUserItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(UserItem.IsSelected))
            {
                OnPropertyChanged(nameof(HasSelection));
                OnPropertyChanged(nameof(SelectedCount));
                
                // Explicitly raise CanExecuteChanged for bulk commands
                (BulkActivateCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (BulkDeactivateCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (BulkDeleteCommand as RelayCommand)?.RaiseCanExecuteChanged();
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
