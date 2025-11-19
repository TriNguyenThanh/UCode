using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using UCode.Desktop.Helpers;
using UCode.Desktop.Models;
using UCode.Desktop.Services;

namespace UCode.Desktop.ViewModels
{
    public class AttendanceDetailViewModel : ViewModelBase
    {
        private readonly AttendanceService _attendanceService;
        private readonly NavigationService _navigationService;
        private string _sessionId = string.Empty;
        private bool _isLoading;
        private AttendanceSessionItem? _session;
        private string _searchText = string.Empty;

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public AttendanceSessionItem? Session
        {
            get => _session;
            set
            {
                if (SetProperty(ref _session, value))
                {
                    OnPropertyChanged(nameof(AttendanceRate));
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterRecords();
                }
            }
        }

        public double AttendanceRate
        {
            get
            {
                if (Session == null || TotalStudents == 0) return 0;
                return (double)Session.AttendedCount / TotalStudents * 100;
            }
        }

        public int TotalStudents { get; private set; } = 50; // TODO: Get from class

        public int ValidCount => Records.Count(r => r.IsValid);
        public int InvalidCount => Records.Count(r => !r.IsValid);

        public ObservableCollection<AttendanceRecord> Records { get; } = new();
        public ObservableCollection<AttendanceRecord> FilteredRecords { get; } = new();

        public ICommand RefreshCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand BackCommand { get; }

        public AttendanceDetailViewModel(
            AttendanceService attendanceService,
            NavigationService navigationService)
        {
            _attendanceService = attendanceService;
            _navigationService = navigationService;

            RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
            ExportCommand = new RelayCommand(async _ => await ExportToExcelAsync());
            BackCommand = new RelayCommand(_ => _navigationService.GoBack());
        }

        public async Task InitializeAsync(string sessionId)
        {
            _sessionId = sessionId;
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                // Load session info
                // TODO: Add GetSessionByIdAsync to service
                // For now, we'll use sample data
                Session = new AttendanceSessionItem
                {
                    SessionId = _sessionId,
                    Title = "Điểm danh tuần 1",
                    SessionCode = "WEEK01",
                    StartTime = DateTime.Now.AddDays(-7),
                    EndTime = DateTime.Now.AddDays(-7).AddHours(2),
                    IsActive = false,
                    AttendedCount = 45,
                    RequireIpCheck = true,
                    AllowedIpSubnet = "192.168.1.0/24",
                    RequireGpsCheck = true,
                    AllowedLatitude = 10.762622m,
                    AllowedLongitude = 106.660172m,
                    AllowedRadiusMeters = 100,
                    CreatedAt = DateTime.Now.AddDays(-7)
                };

                // Load attendance records
                var response = await _attendanceService.GetAttendanceRecordsAsync(_sessionId);
                Records.Clear();
                FilteredRecords.Clear();

                if (response?.Success == true && response.Data != null)
                {
                    foreach (var record in response.Data)
                    {
                        Records.Add(record);
                        FilteredRecords.Add(record);
                    }
                }

                OnPropertyChanged(nameof(AttendanceRate));
                OnPropertyChanged(nameof(ValidCount));
                OnPropertyChanged(nameof(InvalidCount));
            }
            catch (Exception ex)
            {
                await GetMetroWindow()?.ShowMessageAsync(
                    "Lỗi",
                    $"Không thể tải dữ liệu: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }



        private void FilterRecords()
        {
            FilteredRecords.Clear();

            var query = Records.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                query = query.Where(r =>
                    r.StudentCode.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    r.FullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            foreach (var record in query)
            {
                FilteredRecords.Add(record);
            }
        }

        private async Task ExportToExcelAsync()
        {
            await GetMetroWindow()?.ShowMessageAsync(
                "Thông báo",
                "Chức năng xuất Excel đang được phát triển.\n\nDữ liệu sẽ được xuất ra file Excel với các cột:\n" +
                "- MSSV\n" +
                "- Họ và tên\n" +
                "- Thời gian điểm danh\n" +
                "- IP Address\n" +
                "- GPS Location\n" +
                "- Trạng thái");
        }
    }
}
