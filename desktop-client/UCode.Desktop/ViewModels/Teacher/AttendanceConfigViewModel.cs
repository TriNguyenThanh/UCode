using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using UCode.Desktop.Helpers;
using UCode.Desktop.Models;
using UCode.Desktop.Services;

namespace UCode.Desktop.ViewModels
{
    public class AttendanceConfigViewModel : ViewModelBase
    {
        private readonly AttendanceService _attendanceService;
        private readonly string _sessionId;
        private bool _isSaving;
        private string _sessionTitle = string.Empty;

        // Time settings
        private DateTime? _startDate;
        private DateTime? _startTime;
        private DateTime? _endDate;
        private DateTime? _endTime;
        private bool _isActive;

        // IP settings
        private bool _requireIpCheck;
        private string _allowedIpSubnet = string.Empty;

        // GPS settings
        private bool _requireGpsCheck;
        private string _allowedLatitude = string.Empty;
        private string _allowedLongitude = string.Empty;
        private string _allowedRadiusMeters = string.Empty;

        public bool IsSaving
        {
            get => _isSaving;
            set => SetProperty(ref _isSaving, value);
        }

        public string SessionTitle
        {
            get => _sessionTitle;
            set => SetProperty(ref _sessionTitle, value);
        }

        public DateTime? StartDate
        {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }

        public DateTime? StartTime
        {
            get => _startTime;
            set => SetProperty(ref _startTime, value);
        }

        public DateTime? EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        public DateTime? EndTime
        {
            get => _endTime;
            set => SetProperty(ref _endTime, value);
        }

        public bool RequireIpCheck
        {
            get => _requireIpCheck;
            set => SetProperty(ref _requireIpCheck, value);
        }

        public string AllowedIpSubnet
        {
            get => _allowedIpSubnet;
            set => SetProperty(ref _allowedIpSubnet, value);
        }

        public bool RequireGpsCheck
        {
            get => _requireGpsCheck;
            set => SetProperty(ref _requireGpsCheck, value);
        }

        public string AllowedLatitude
        {
            get => _allowedLatitude;
            set => SetProperty(ref _allowedLatitude, value);
        }

        public string AllowedLongitude
        {
            get => _allowedLongitude;
            set => SetProperty(ref _allowedLongitude, value);
        }

        public string AllowedRadiusMeters
        {
            get => _allowedRadiusMeters;
            set => SetProperty(ref _allowedRadiusMeters, value);
        }

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand OpenMapCommand { get; }

        public AttendanceConfigViewModel(AttendanceService attendanceService, string sessionId, AttendanceSessionItem session)
        {
            _attendanceService = attendanceService;
            _sessionId = sessionId;

            SaveCommand = new RelayCommand(async _ => await SaveConfigAsync());
            OpenMapCommand = new RelayCommand(_ => OpenMap());

            // Initialize from session
            SessionTitle = session.Title;
            IsActive = session.IsActive;
            StartDate = session.StartTime.Date;
            StartTime = new DateTime(session.StartTime.Year, session.StartTime.Month, session.StartTime.Day, 
                                     session.StartTime.Hour, session.StartTime.Minute, 0);
            EndDate = session.EndTime.Date;
            EndTime = new DateTime(session.EndTime.Year, session.EndTime.Month, session.EndTime.Day, 
                                   session.EndTime.Hour, session.EndTime.Minute, 0);

            RequireIpCheck = session.RequireIpCheck;
            AllowedIpSubnet = session.AllowedIpSubnet ?? string.Empty;

            RequireGpsCheck = session.RequireGpsCheck;
            AllowedLatitude = session.AllowedLatitude?.ToString() ?? string.Empty;
            AllowedLongitude = session.AllowedLongitude?.ToString() ?? string.Empty;
            AllowedRadiusMeters = session.AllowedRadiusMeters?.ToString() ?? string.Empty;

            // Auto-fill IP and GPS defaults when enabled
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(RequireIpCheck) && RequireIpCheck)
                {
                    if (string.IsNullOrWhiteSpace(AllowedIpSubnet))
                    {
                        AllowedIpSubnet = GetPublicIp();
                    }
                }

                if (e.PropertyName == nameof(RequireGpsCheck) && RequireGpsCheck)
                {
                    if (string.IsNullOrWhiteSpace(AllowedRadiusMeters))
                    {
                        AllowedRadiusMeters = "20";
                    }
                }
            };
        }

        private async Task SaveConfigAsync()
        {
            // Validation
            if (string.IsNullOrWhiteSpace(SessionTitle))
            {
                await GetMetroWindow()?.ShowMessageAsync("Thông báo", "Vui lòng nhập tiêu đề phiên điểm danh");
                return;
            }

            if (!StartDate.HasValue || !StartTime.HasValue || !EndDate.HasValue || !EndTime.HasValue)
            {
                await GetMetroWindow()?.ShowMessageAsync("Thông báo", "Vui lòng nhập đầy đủ thời gian bắt đầu và kết thúc");
                return;
            }

            var startDateTime = StartDate.Value.Date + StartTime.Value.TimeOfDay;
            var endDateTime = EndDate.Value.Date + EndTime.Value.TimeOfDay;

            if (endDateTime <= startDateTime)
            {
                await GetMetroWindow()?.ShowMessageAsync("Thông báo", "Thời gian kết thúc phải sau thời gian bắt đầu");
                return;
            }

            if (RequireIpCheck && string.IsNullOrWhiteSpace(AllowedIpSubnet))
            {
                await GetMetroWindow()?.ShowMessageAsync("Thông báo", "Vui lòng nhập IP khi bật kiểm tra IP");
                return;
            }

            if (RequireGpsCheck)
            {
                if (string.IsNullOrWhiteSpace(AllowedLatitude) || 
                    string.IsNullOrWhiteSpace(AllowedLongitude) || 
                    string.IsNullOrWhiteSpace(AllowedRadiusMeters))
                {
                    await GetMetroWindow()?.ShowMessageAsync("Thông báo", "Vui lòng nhập đầy đủ thông tin GPS khi bật kiểm tra vị trí");
                    return;
                }

                if (!decimal.TryParse(AllowedLatitude, out _) || 
                    !decimal.TryParse(AllowedLongitude, out _) ||
                    !int.TryParse(AllowedRadiusMeters, out _))
                {
                    await GetMetroWindow()?.ShowMessageAsync("Thông báo", "Thông tin GPS không hợp lệ");
                    return;
                }
            }

            IsSaving = true;

            try
            {
                var request = new Services.UpdateAttendanceSessionRequest
                {
                    Title = SessionTitle?.Trim(),
                    IsActive = IsActive,
                    StartTime = startDateTime,
                    EndTime = endDateTime,
                    RequireIpCheck = RequireIpCheck,
                    AllowedIpSubnet = RequireIpCheck ? AllowedIpSubnet.Trim() : null,
                    RequireGpsCheck = RequireGpsCheck,
                    AllowedLatitude = RequireGpsCheck ? decimal.Parse(AllowedLatitude) : null,
                    AllowedLongitude = RequireGpsCheck ? decimal.Parse(AllowedLongitude) : null,
                    AllowedRadiusMeters = RequireGpsCheck ? int.Parse(AllowedRadiusMeters) : null
                };

                var response = await _attendanceService.UpdateAttendanceSessionAsync(_sessionId, request);

                if (response?.Success == true)
                {
                    await GetMetroWindow()?.ShowMessageAsync("Thành công", "Đã cập nhật cấu hình điểm danh!");

                    // Close dialog with success
                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window.DataContext == this)
                        {
                            window.DialogResult = true;
                            window.Close();
                            break;
                        }
                    }
                }
                else
                {
                    await GetMetroWindow()?.ShowMessageAsync("Lỗi", $"Cập nhật thất bại: {response?.Message}");
                }
            }
            catch (Exception ex)
            {
                await GetMetroWindow()?.ShowMessageAsync("Lỗi", $"Lỗi: {ex.Message}");
            }
            finally
            {
                IsSaving = false;
            }
        }

        private async void OpenMap()
        {
            try
            {
                // Open Google Maps to get coordinates
                var url = "https://www.google.com/maps/@?api=1&map_action=map";
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });

                await GetMetroWindow()?.ShowMessageAsync(
                    "Hướng dẫn lấy tọa độ từ Google Maps",
                    "1. Click chuột phải vào vị trí bạn muốn trên bản đồ\n" +
                    "2. Click vào tọa độ đầu tiên trong menu (dạng: 10.762622, 106.660172)\n" +
                    "3. Tọa độ sẽ được copy vào clipboard\n" +
                    "4. Paste vào ô 'Vĩ độ, Kinh độ' bên dưới\n\n" +
                    "Hoặc xem tọa độ trong URL: @10.762622,106.660172");
            }
            catch (Exception ex)
            {
                await GetMetroWindow()?.ShowMessageAsync(
                    "Lỗi",
                    $"Không thể mở Google Maps: {ex.Message}");
            }
        }

        private string GetPublicIp()
        {
            try
            {
                // Get public IP from external service
                using (var client = new System.Net.WebClient())
                {
                    var publicIp = client.DownloadString("https://api.ipify.org").Trim();
                    if (!string.IsNullOrWhiteSpace(publicIp))
                    {
                        return publicIp;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting public IP: {ex.Message}");
                
                // Fallback: Try to get local network IP (WiFi/Ethernet)
                try
                {
                    var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                    foreach (var ip in host.AddressList)
                    {
                        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            var ipString = ip.ToString();
                            // Skip loopback
                            if (!ipString.StartsWith("127."))
                            {
                                return ipString;
                            }
                        }
                    }
                }
                catch (Exception ex2)
                {
                    System.Diagnostics.Debug.WriteLine($"Error getting local IP: {ex2.Message}");
                }
            }

            // Default fallback
            return "";
        }
    }
}
