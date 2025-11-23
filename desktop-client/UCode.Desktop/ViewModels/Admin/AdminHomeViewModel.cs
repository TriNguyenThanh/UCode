using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using UCode.Desktop.Helpers;
using UCode.Desktop.Models.Admin;
using UCode.Desktop.Services;
using UCode.Desktop.Services.Admin;

namespace UCode.Desktop.ViewModels.Admin
{
    /// <summary>
    /// ViewModel cho Admin Dashboard
    /// </summary>
    public class AdminHomeViewModel : INotifyPropertyChanged
    {
        private readonly AdminStatisticsService _statisticsService;
        private readonly NavigationService _navigationService;
        private readonly IDialogCoordinator _dialogCoordinator;
        
        private bool _isLoading;
        private SystemStatistics? _systemStats;
        private UserStatistics? _userStats;
        private ClassStatistics? _classStats;

        #region Properties

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        // System Statistics
        public int TotalUsers => _systemStats?.TotalUsers ?? 0;
        public int TotalStudents => _systemStats?.TotalStudents ?? 0;
        public int TotalTeachers => _systemStats?.TotalTeachers ?? 0;
        public int TotalAdmins => _systemStats?.TotalAdmins ?? 0;
        public int TotalClasses => _systemStats?.TotalClasses ?? 0;
        public int TotalActiveClasses => _systemStats?.TotalActiveClasses ?? 0;
        public int TotalArchivedClasses => _systemStats?.TotalArchivedClasses ?? 0;
        public int TotalProblems => _systemStats?.TotalProblems ?? 0;
        public int TotalSubmissions => _systemStats?.TotalSubmissions ?? 0;

        // User Statistics
        public int ActiveUsers => _userStats?.ActiveUsers ?? 0;
        public int InactiveUsers => _userStats?.InactiveUsers ?? 0;
        public int BannedUsers => _userStats?.BannedUsers ?? 0;
        public int NewUsersToday => _userStats?.NewUsersToday ?? 0;
        public int NewUsersThisWeek => _userStats?.NewUsersThisWeek ?? 0;
        public int NewUsersThisMonth => _userStats?.NewUsersThisMonth ?? 0;

        // Class Statistics
        public int InactiveClasses => _classStats?.InactiveClasses ?? 0;
        public double AverageStudentsPerClass => _classStats?.AverageStudentsPerClass ?? 0;
        public int ClassesCreatedToday => _classStats?.ClassesCreatedToday ?? 0;
        public int ClassesCreatedThisWeek => _classStats?.ClassesCreatedThisWeek ?? 0;
        public int ClassesCreatedThisMonth => _classStats?.ClassesCreatedThisMonth ?? 0;

        #endregion

        #region Commands

        public ICommand RefreshCommand { get; }
        public ICommand NavigateToUsersCommand { get; }
        public ICommand NavigateToClassesCommand { get; }
        public ICommand NavigateToProblemsCommand { get; }
        public ICommand NavigateToLogsCommand { get; }
        public ICommand NavigateToSettingsCommand { get; }

        #endregion

        public AdminHomeViewModel(
            AdminStatisticsService statisticsService,
            NavigationService navigationService,
            IDialogCoordinator dialogCoordinator)
        {
            _statisticsService = statisticsService;
            _navigationService = navigationService;
            _dialogCoordinator = dialogCoordinator;

            RefreshCommand = new RelayCommand(async _ => await LoadStatisticsAsync());
            NavigateToUsersCommand = new RelayCommand(_ => NavigateToUsers());
            NavigateToClassesCommand = new RelayCommand(_ => NavigateToClasses());
            NavigateToProblemsCommand = new RelayCommand(_ => NavigateToProblems());
            NavigateToLogsCommand = new RelayCommand(_ => NavigateToLogs());
            NavigateToSettingsCommand = new RelayCommand(_ => NavigateToSettings());
        }

        public async Task InitializeAsync()
        {
            await LoadStatisticsAsync();
        }

        public async Task LoadStatisticsAsync()
        {
            IsLoading = true;

            try
            {
                System.Diagnostics.Debug.WriteLine("=== Starting LoadStatisticsAsync ===");
                
                // Load statistics in parallel for faster loading
                System.Diagnostics.Debug.WriteLine("Fetching statistics from API...");
                var userStatsTask = _statisticsService.GetUserStatisticsAsync();
                var classStatsTask = _statisticsService.GetClassStatisticsAsync();
                
                await Task.WhenAll(userStatsTask, classStatsTask);
                
                _userStats = await userStatsTask;
                _classStats = await classStatsTask;
                
                System.Diagnostics.Debug.WriteLine($"User stats: {_userStats?.TotalUsers ?? 0} users");
                System.Diagnostics.Debug.WriteLine($"Class stats: {_classStats?.TotalClasses ?? 0} classes");

                // Build system statistics from combined data
                if (_userStats != null && _classStats != null)
                {
                    _systemStats = new Models.Admin.SystemStatistics
                    {
                        TotalUsers = _userStats.TotalUsers,
                        TotalStudents = _userStats.StudentCount,
                        TotalTeachers = _userStats.TeacherCount,
                        TotalAdmins = _userStats.AdminCount,
                        TotalClasses = _classStats.TotalClasses,
                        TotalActiveClasses = _classStats.ActiveClasses,
                        TotalArchivedClasses = _classStats.ArchivedClasses,
                        TotalProblems = 0,
                        TotalSubmissions = 0,
                        TodayActiveUsers = 0,
                        WeekActiveUsers = 0,
                        MonthActiveUsers = 0
                    };
                }

                // Notify all properties changed
                OnPropertyChanged(string.Empty);
                System.Diagnostics.Debug.WriteLine("=== LoadStatisticsAsync completed successfully ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in LoadStatisticsAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                
                await _dialogCoordinator.ShowMessageAsync(
                    this,
                    "Lỗi",
                    $"Không thể tải thống kê: {ex.Message}",
                    MessageDialogStyle.Affirmative
                );
            }
            finally
            {
                IsLoading = false;
                System.Diagnostics.Debug.WriteLine("IsLoading set to false");
            }
        }

        private void NavigateToUsers()
        {
            // Navigation is handled by AdminMainWindow, not here
        }

        private void NavigateToClasses()
        {
            // Navigation is handled by AdminMainWindow, not here
        }

        private void NavigateToProblems()
        {
            // Navigation is handled by AdminMainWindow, not here
        }

        private void NavigateToLogs()
        {
            // Navigation is handled by AdminMainWindow, not here
        }

        private void NavigateToSettings()
        {
            // Navigation is handled by AdminMainWindow, not here
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
