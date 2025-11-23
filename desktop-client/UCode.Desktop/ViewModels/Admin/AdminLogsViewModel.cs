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
using UCode.Desktop.Models.Admin;
using UCode.Desktop.Services.Admin;

namespace UCode.Desktop.ViewModels.Admin
{
    /// <summary>
    /// ViewModel cho Admin Logs Management
    /// </summary>
    public class AdminLogsViewModel : INotifyPropertyChanged
    {
        private readonly AdminLogsService _logsService;
        private readonly IDialogCoordinator _dialogCoordinator;

        private bool _isLoading;
        private string _searchTerm = string.Empty;
        private string _selectedLogType = "System";
        private string _selectedLevel = "All";
        private string _selectedService = "All";
        private DateTime? _startDate;
        private DateTime? _endDate;
        private int _currentPage = 1;
        private int _totalPages = 1;
        private int _totalLogs = 0;
        private const int PageSize = 50;

        private Dictionary<string, int>? _logStatistics;

        public ObservableCollection<SystemLog> SystemLogs { get; } = new();
        public ObservableCollection<ActivityLog> ActivityLogs { get; } = new();
        public ObservableCollection<string> AvailableServices { get; } = new();

        public List<string> LogTypes { get; } = new() { "System", "Activity" };
        public List<string> LogLevels { get; } = new() { "All", "Info", "Warning", "Error" };

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
                    _ = SearchLogsAsync();
                }
            }
        }

        public string SelectedLogType
        {
            get => _selectedLogType;
            set
            {
                if (SetProperty(ref _selectedLogType, value))
                {
                    _ = LoadLogsAsync();
                }
            }
        }

        public string SelectedLevel
        {
            get => _selectedLevel;
            set
            {
                if (SetProperty(ref _selectedLevel, value))
                {
                    _ = LoadLogsAsync();
                }
            }
        }

        public string SelectedService
        {
            get => _selectedService;
            set
            {
                if (SetProperty(ref _selectedService, value))
                {
                    _ = LoadLogsAsync();
                }
            }
        }

        public DateTime? StartDate
        {
            get => _startDate;
            set
            {
                if (SetProperty(ref _startDate, value))
                {
                    _ = LoadLogsAsync();
                }
            }
        }

        public DateTime? EndDate
        {
            get => _endDate;
            set
            {
                if (SetProperty(ref _endDate, value))
                {
                    _ = LoadLogsAsync();
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

        public int TotalLogs
        {
            get => _totalLogs;
            set => SetProperty(ref _totalLogs, value);
        }

        // Statistics
        public int TotalInfoLogs => _logStatistics?.GetValueOrDefault("Info", 0) ?? 0;
        public int TotalWarningLogs => _logStatistics?.GetValueOrDefault("Warning", 0) ?? 0;
        public int TotalErrorLogs => _logStatistics?.GetValueOrDefault("Error", 0) ?? 0;

        #endregion

        #region Commands

        public ICommand LoadLogsCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ExportLogsCommand { get; }
        public ICommand ClearFiltersCommand { get; }
        public ICommand DeleteOldLogsCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand RefreshCommand { get; }

        #endregion

        public AdminLogsViewModel(
            AdminLogsService logsService,
            IDialogCoordinator dialogCoordinator)
        {
            _logsService = logsService;
            _dialogCoordinator = dialogCoordinator;

            LoadLogsCommand = new RelayCommand(async _ => await LoadLogsAsync());
            SearchCommand = new RelayCommand(async _ => await SearchLogsAsync());
            ExportLogsCommand = new RelayCommand(async _ => await ExportLogsAsync());
            ClearFiltersCommand = new RelayCommand(_ => ClearFilters());
            DeleteOldLogsCommand = new RelayCommand(async _ => await DeleteOldLogsAsync());
            NextPageCommand = new RelayCommand(async _ => await NextPageAsync(), _ => CurrentPage < TotalPages);
            PreviousPageCommand = new RelayCommand(async _ => await PreviousPageAsync(), _ => CurrentPage > 1);
            RefreshCommand = new RelayCommand(async _ => await LoadLogsAsync());
        }

        public async Task InitializeAsync()
        {
            await LoadAvailableServicesAsync();
            await LoadStatisticsAsync();
            await LoadLogsAsync();
        }

        private async Task LoadAvailableServicesAsync()
        {
            try
            {
                var services = await _logsService.GetAvailableServicesAsync();
                if (services != null)
                {
                    AvailableServices.Clear();
                    AvailableServices.Add("All");
                    foreach (var service in services)
                    {
                        AvailableServices.Add(service);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load available services: {ex.Message}");
            }
        }

        private async Task LoadStatisticsAsync()
        {
            try
            {
                _logStatistics = await _logsService.GetLogStatisticsAsync(_startDate, _endDate);
                OnPropertyChanged(nameof(TotalInfoLogs));
                OnPropertyChanged(nameof(TotalWarningLogs));
                OnPropertyChanged(nameof(TotalErrorLogs));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load log statistics: {ex.Message}");
            }
        }

        public async Task LoadLogsAsync()
        {
            IsLoading = true;

            try
            {
                if (SelectedLogType == "System")
                {
                    var response = await _logsService.GetSystemLogsAsync(
                        level: SelectedLevel == "All" ? null : SelectedLevel,
                        service: SelectedService == "All" ? null : SelectedService,
                        startDate: StartDate,
                        endDate: EndDate,
                        searchTerm: string.IsNullOrWhiteSpace(SearchTerm) ? null : SearchTerm,
                        pageNumber: CurrentPage,
                        pageSize: PageSize
                    );

                    if (response != null)
                    {
                        SystemLogs.Clear();
                        foreach (var log in response.Items)
                        {
                            SystemLogs.Add(log);
                        }

                        TotalPages = response.TotalPages;
                        TotalLogs = response.TotalCount;
                    }
                }
                else // Activity
                {
                    var response = await _logsService.GetActivityLogsAsync(
                        startDate: StartDate,
                        endDate: EndDate,
                        searchTerm: string.IsNullOrWhiteSpace(SearchTerm) ? null : SearchTerm,
                        pageNumber: CurrentPage,
                        pageSize: PageSize
                    );

                    if (response != null)
                    {
                        ActivityLogs.Clear();
                        foreach (var log in response.Items)
                        {
                            ActivityLogs.Add(log);
                        }

                        TotalPages = response.TotalPages;
                        TotalLogs = response.TotalCount;
                    }
                }

                await LoadStatisticsAsync();
            }
            catch (Exception ex)
            {
                await _dialogCoordinator.ShowMessageAsync(
                    this,
                    "Lỗi",
                    $"Không thể tải logs: {ex.Message}",
                    MessageDialogStyle.Affirmative
                );
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchLogsAsync()
        {
            CurrentPage = 1;
            await LoadLogsAsync();
        }

        private async Task ExportLogsAsync()
        {
            try
            {
                var result = await _dialogCoordinator.ShowMessageAsync(
                    this,
                    "Xuất Logs",
                    "Bạn có muốn xuất logs ra file Excel không?",
                    MessageDialogStyle.AffirmativeAndNegative
                );

                if (result == MessageDialogResult.Affirmative)
                {
                    IsLoading = true;
                    var data = await _logsService.ExportLogsAsync(
                        logType: SelectedLogType.ToLower(),
                        level: SelectedLevel == "All" ? null : SelectedLevel,
                        service: SelectedService == "All" ? null : SelectedService,
                        startDate: StartDate,
                        endDate: EndDate,
                        format: "xlsx"
                    );

                    if (data != null)
                    {
                        var dialog = new Microsoft.Win32.SaveFileDialog
                        {
                            FileName = $"logs_{DateTime.Now:yyyyMMdd_HHmmss}",
                            DefaultExt = ".xlsx",
                            Filter = "Excel Files (*.xlsx)|*.xlsx"
                        };

                        if (dialog.ShowDialog() == true)
                        {
                            await System.IO.File.WriteAllBytesAsync(dialog.FileName, data);
                            await _dialogCoordinator.ShowMessageAsync(
                                this,
                                "Thành công",
                                "Đã xuất logs thành công!",
                                MessageDialogStyle.Affirmative
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await _dialogCoordinator.ShowMessageAsync(
                    this,
                    "Lỗi",
                    $"Không thể xuất logs: {ex.Message}",
                    MessageDialogStyle.Affirmative
                );
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ClearFilters()
        {
            SearchTerm = string.Empty;
            SelectedLevel = "All";
            SelectedService = "All";
            StartDate = null;
            EndDate = null;
        }

        private async Task DeleteOldLogsAsync()
        {
            try
            {
                var result = await _dialogCoordinator.ShowMessageAsync(
                    this,
                    "Xóa Logs Cũ",
                    "Bạn có chắc chắn muốn xóa tất cả logs cũ hơn 30 ngày?",
                    MessageDialogStyle.AffirmativeAndNegative
                );

                if (result == MessageDialogResult.Affirmative)
                {
                    IsLoading = true;
                    var beforeDate = DateTime.Now.AddDays(-30);
                    await _logsService.DeleteOldLogsAsync(beforeDate);

                    await _dialogCoordinator.ShowMessageAsync(
                        this,
                        "Thành công",
                        "Đã xóa logs cũ thành công!",
                        MessageDialogStyle.Affirmative
                    );

                    await LoadLogsAsync();
                }
            }
            catch (Exception ex)
            {
                await _dialogCoordinator.ShowMessageAsync(
                    this,
                    "Lỗi",
                    $"Không thể xóa logs: {ex.Message}",
                    MessageDialogStyle.Affirmative
                );
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task NextPageAsync()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                await LoadLogsAsync();
            }
        }

        private async Task PreviousPageAsync()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                await LoadLogsAsync();
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
