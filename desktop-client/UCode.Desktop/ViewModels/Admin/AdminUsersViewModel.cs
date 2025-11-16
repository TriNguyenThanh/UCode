using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UCode.Desktop.Helpers;
using UCode.Desktop.Models.Admin;
using UCode.Desktop.Models.Enums;
using UCode.Desktop.Services;

namespace UCode.Desktop.ViewModels.Admin
{
    public class AdminUsersViewModel : INotifyPropertyChanged
    {
        private readonly AdminService _adminService;
        private ObservableCollection<UserManagement> _users = new();
        private UserManagement? _selectedUser;
        private string _searchText = string.Empty;
        private UserRole? _selectedRoleFilter;
        private bool? _selectedStatusFilter;
        private bool _isLoading;
        private int _currentPage = 1;
        private int _pageSize = 20;

        public AdminUsersViewModel(AdminService adminService)
        {
            _adminService = adminService;

            SearchCommand = new RelayCommand(async _ => await SearchUsersAsync());
            CreateUserCommand = new RelayCommand(_ => CreateUser());
            EditUserCommand = new RelayCommand(async _ => await EditUserAsync(), _ => SelectedUser != null);
            DeleteUserCommand = new RelayCommand(async _ => await DeleteUserAsync(), _ => SelectedUser != null);
            ResetPasswordCommand = new RelayCommand(async _ => await ResetPasswordAsync(), _ => SelectedUser != null);
            RefreshCommand = new RelayCommand(async _ => await LoadUsersAsync());
            ExportCommand = new RelayCommand(async _ => await ExportUsersAsync());
            BulkActivateCommand = new RelayCommand(async _ => await BulkActivateAsync());
            BulkDeactivateCommand = new RelayCommand(async _ => await BulkDeactivateAsync());
            NextPageCommand = new RelayCommand(async _ => await NextPageAsync());
            PreviousPageCommand = new RelayCommand(async _ => await PreviousPageAsync(), _ => CurrentPage > 1);
        }

        #region Properties

        public ObservableCollection<UserManagement> Users
        {
            get => _users;
            set
            {
                _users = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(UsersCount));
            }
        }

        public int UsersCount => _users?.Count ?? 0;

        public UserManagement? SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                OnPropertyChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
            }
        }

        public UserRole? SelectedRoleFilter
        {
            get => _selectedRoleFilter;
            set
            {
                _selectedRoleFilter = value;
                OnPropertyChanged();
            }
        }

        public bool? SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set
            {
                _selectedStatusFilter = value;
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

        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged();
            }
        }

        public int PageSize
        {
            get => _pageSize;
            set
            {
                _pageSize = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        public ICommand SearchCommand { get; }
        public ICommand CreateUserCommand { get; }
        public ICommand EditUserCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand ResetPasswordCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand BulkActivateCommand { get; }
        public ICommand BulkDeactivateCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }

        #endregion

        #region Methods

        public async Task LoadUsersAsync()
        {
            await SearchUsersAsync();
        }

        private async Task SearchUsersAsync()
        {
            try
            {
                IsLoading = true;
                System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] Loading users - Page: {CurrentPage}, Search: '{SearchText}'");
                var users = await _adminService.GetAllUsersAsync(
                    SearchText,
                    SelectedRoleFilter?.ToString(),
                    SelectedStatusFilter,
                    CurrentPage,
                    PageSize);

                Users = new ObservableCollection<UserManagement>(users);
                System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] Loaded {users.Count} users");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminUsersViewModel] Error: {ex}");
                ModernMessageBox.Show(
                    $"Không thể tải danh sách người dùng.\n\nLỗi: {ex.Message}\n\nVui lòng kiểm tra backend đang chạy.",
                    "Lỗi Tải Dữ Liệu",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CreateUser()
        {
            var dialog = new Views.Dialogs.UserCreateDialog(_adminService);
            dialog.Owner = Application.Current.MainWindow;
            
            if (dialog.ShowDialog() == true)
            {
                // Refresh the list
                _ = LoadUsersAsync();
            }
        }

        private async Task EditUserAsync()
        {
            if (SelectedUser == null) return;

            var dialog = new Views.Dialogs.UserEditDialog(_adminService, SelectedUser);
            dialog.Owner = Application.Current.MainWindow;
            
            if (dialog.ShowDialog() == true)
            {
                // Refresh the list
                await LoadUsersAsync();
            }
        }

        private async Task DeleteUserAsync()
        {
            if (SelectedUser == null) return;

            var result = ModernMessageBox.Show(
                $"Are you sure you want to delete user '{SelectedUser.FullName}'?\nThis action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    var success = await _adminService.DeleteUserAsync(SelectedUser.Id);
                    
                    if (success)
                    {
                        ModernMessageBox.Show(
                            "User deleted successfully",
                            "Success",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        await LoadUsersAsync();
                    }
                    else
                    {
                        ModernMessageBox.Show(
                            "Failed to delete user",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    ModernMessageBox.Show(
                        $"Error deleting user: {ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private async Task ResetPasswordAsync()
        {
            if (SelectedUser == null) return;

            var dialog = new Views.Dialogs.PasswordResetDialog(_adminService, SelectedUser);
            dialog.Owner = Application.Current.MainWindow;
            
            if (dialog.ShowDialog() == true)
            {
                // Password reset successful
                ModernMessageBox.Show(
                    $"Password for '{SelectedUser.FullName}' has been reset.",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private async Task ExportUsersAsync()
        {
            try
            {
                IsLoading = true;
                var data = await _adminService.ExportUsersToExcelAsync();
                
                if (data != null && data.Length > 0)
                {
                    // Show save file dialog
                    var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                    {
                        Filter = "Excel Files (*.xlsx)|*.xlsx",
                        FileName = $"Users_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                        DefaultExt = ".xlsx"
                    };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        await System.IO.File.WriteAllBytesAsync(saveFileDialog.FileName, data);
                        
                        ModernMessageBox.Show(
                            $"Users exported successfully to:\n{saveFileDialog.FileName}",
                            "Success",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }
                else
                {
                    ModernMessageBox.Show(
                        "No data to export",
                        "Warning",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                ModernMessageBox.Show(
                    $"Export failed: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task BulkActivateAsync()
        {
            // TODO: Get selected users and activate them
            ModernMessageBox.Show(
                "Bulk activate will be implemented",
                "Info",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private async Task BulkDeactivateAsync()
        {
            // TODO: Get selected users and deactivate them
            ModernMessageBox.Show(
                "Bulk deactivate will be implemented",
                "Info",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private async Task NextPageAsync()
        {
            CurrentPage++;
            await LoadUsersAsync();
        }

        private async Task PreviousPageAsync()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                await LoadUsersAsync();
            }
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
