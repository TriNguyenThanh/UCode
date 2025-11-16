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
using UCode.Desktop.Services;

namespace UCode.Desktop.ViewModels.Admin
{
    public class AdminHomeViewModel : INotifyPropertyChanged
    {
        private readonly AdminService _adminService;
        private readonly NavigationService _navigationService;
        private AdminDashboardStats? _stats;
        private bool _isLoading;

        public AdminHomeViewModel(AdminService adminService, NavigationService navigationService)
        {
            _adminService = adminService;
            _navigationService = navigationService;

            RefreshCommand = new RelayCommand(async _ => await LoadDashboardDataAsync());
            NavigateToUsersCommand = new RelayCommand(_ => NavigateToUsers());
            NavigateToClassesCommand = new RelayCommand(_ => NavigateToClasses());
            NavigateToProblemsCommand = new RelayCommand(_ => NavigateToProblems());
            NavigateToSettingsCommand = new RelayCommand(_ => NavigateToSettings());
        }

        #region Properties

        public AdminDashboardStats? Stats
        {
            get => _stats;
            set
            {
                _stats = value;
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

        #endregion

        #region Commands

        public ICommand RefreshCommand { get; }
        public ICommand NavigateToUsersCommand { get; }
        public ICommand NavigateToClassesCommand { get; }
        public ICommand NavigateToProblemsCommand { get; }
        public ICommand NavigateToSettingsCommand { get; }

        #endregion

        #region Methods

        public async Task LoadDashboardDataAsync()
        {
            try
            {
                IsLoading = true;
                System.Diagnostics.Debug.WriteLine("[AdminHomeViewModel] Loading dashboard data...");
                Stats = await _adminService.GetDashboardStatsAsync();
                System.Diagnostics.Debug.WriteLine($"[AdminHomeViewModel] Dashboard loaded - Users: {Stats?.TotalUsers}, Classes: {Stats?.TotalClasses}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminHomeViewModel] Error: {ex}");
                ModernMessageBox.Show(
                    $"Không thể tải dữ liệu bảng điều khiển.\n\nLỗi: {ex.Message}\n\nVui lòng kiểm tra:\n1. Backend đang chạy tại http://localhost:5000\n2. Bạn đã đăng nhập với tài khoản Admin\n3. Kết nối mạng ổn định",
                    "Lỗi Tải Dữ Liệu",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void NavigateToUsers()
        {
            _navigationService.NavigateTo(new UCode.Desktop.Pages.Admin.AdminUsersPage(_adminService));
        }

        private void NavigateToClasses()
        {
            _navigationService.NavigateTo(new UCode.Desktop.Pages.Admin.AdminClassesPage(_adminService));
        }

        private void NavigateToProblems()
        {
            _navigationService.NavigateTo(new UCode.Desktop.Pages.Admin.AdminProblemsPage(_adminService));
        }

        private void NavigateToSettings()
        {
            _navigationService.NavigateTo(new UCode.Desktop.Pages.Admin.AdminSettingsPage(_adminService));
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
