using System;
using System.Threading.Tasks;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using UCode.Desktop.Helpers;
using UCode.Desktop.Services;

namespace UCode.Desktop.ViewModels
{
    public class CreateAttendanceSessionViewModel : ViewModelBase
    {
        private readonly AttendanceService _attendanceService;
        private readonly NavigationService _navigationService;
        private string _classId = string.Empty;
        private bool _isLoading;
        private string _title = string.Empty;
        private string _sessionCode = string.Empty;
        private DateTime _startTime = DateTime.Now;
        private DateTime _endTime = DateTime.Now.AddHours(2);
        private bool _isActive = true;
        private bool _requireIpCheck;
        private string _allowedIpSubnet = string.Empty;
        private bool _requireGpsCheck;
        private string _allowedLatitude = string.Empty;
        private string _allowedLongitude = string.Empty;
        private string _allowedRadiusMeters = string.Empty;

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string SessionCode
        {
            get => _sessionCode;
            set => SetProperty(ref _sessionCode, value);
        }

        public DateTime StartTime
        {
            get => _startTime;
            set => SetProperty(ref _startTime, value);
        }

        public DateTime EndTime
        {
            get => _endTime;
            set => SetProperty(ref _endTime, value);
        }

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
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

        public ICommand CreateCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand GetLocationCommand { get; }

        public CreateAttendanceSessionViewModel(
            AttendanceService attendanceService,
            NavigationService navigationService)
        {
            _attendanceService = attendanceService;
            _navigationService = navigationService;

            CreateCommand = new RelayCommand(async _ => await CreateSessionAsync());
            CancelCommand = new RelayCommand(_ => NavigateBack());
            BackCommand = new RelayCommand(_ => NavigateBack());
            GetLocationCommand = new RelayCommand(async _ => await GetCurrentLocationAsync());

            // Auto-fill IP and GPS when enabled
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(RequireIpCheck) && RequireIpCheck)
                {
                    if (string.IsNullOrWhiteSpace(AllowedIpSubnet))
                    {
                        AllowedIpSubnet = GetPublicIp();
                    }
                }
                else if (e.PropertyName == nameof(RequireGpsCheck) && RequireGpsCheck)
                {
                    if (string.IsNullOrWhiteSpace(AllowedRadiusMeters))
                    {
                        AllowedRadiusMeters = "20";
                    }
                }
            };
        }

        public void Initialize(string classId)
        {
            _classId = classId;
        }

        private async Task GetCurrentLocationAsync()
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

        private async Task CreateSessionAsync()
        {
            // Validation
            if (string.IsNullOrWhiteSpace(Title))
            {
                await GetMetroWindow()?.ShowMessageAsync("Lỗi", "Vui lòng nhập tiêu đề phiên điểm danh.");
                return;
            }

            if (EndTime <= StartTime)
            {
                await GetMetroWindow()?.ShowMessageAsync("Lỗi", "Thời gian kết thúc phải sau thời gian bắt đầu.");
                return;
            }

            if (RequireIpCheck && string.IsNullOrWhiteSpace(AllowedIpSubnet))
            {
                await GetMetroWindow()?.ShowMessageAsync("Lỗi", "Vui lòng nhập dải IP cho phép.");
                return;
            }

            if (RequireGpsCheck)
            {
                if (string.IsNullOrWhiteSpace(AllowedLatitude) || 
                    string.IsNullOrWhiteSpace(AllowedLongitude) ||
                    string.IsNullOrWhiteSpace(AllowedRadiusMeters))
                {
                    await GetMetroWindow()?.ShowMessageAsync("Lỗi", "Vui lòng nhập đầy đủ thông tin GPS.");
                    return;
                }

                if (!decimal.TryParse(AllowedLatitude, out _) || 
                    !decimal.TryParse(AllowedLongitude, out _))
                {
                    await GetMetroWindow()?.ShowMessageAsync("Lỗi", "Tọa độ GPS không hợp lệ.");
                    return;
                }

                if (!int.TryParse(AllowedRadiusMeters, out _))
                {
                    await GetMetroWindow()?.ShowMessageAsync("Lỗi", "Bán kính phải là số nguyên.");
                    return;
                }
            }

            IsLoading = true;
            try
            {
                var request = new CreateAttendanceSessionRequest
                {
                    ClassId = _classId,
                    Title = Title,
                    SessionCode = SessionCode,
                    StartTime = StartTime,
                    EndTime = EndTime,
                    IsActive = IsActive,
                    RequireIpCheck = RequireIpCheck,
                    AllowedIpSubnet = RequireIpCheck ? AllowedIpSubnet : null,
                    RequireGpsCheck = RequireGpsCheck,
                    AllowedLatitude = RequireGpsCheck ? decimal.Parse(AllowedLatitude) : null,
                    AllowedLongitude = RequireGpsCheck ? decimal.Parse(AllowedLongitude) : null,
                    AllowedRadiusMeters = RequireGpsCheck ? int.Parse(AllowedRadiusMeters) : null
                };

                var response = await _attendanceService.CreateAttendanceSessionAsync(request);

                if (response?.Success == true)
                {
                    await GetMetroWindow()?.ShowMessageAsync(
                        "Thành công",
                        $"Đã tạo phiên điểm danh '{Title}' thành công!");
                    
                    NavigateBack();
                }
                else
                {
                    await GetMetroWindow()?.ShowMessageAsync(
                        "Lỗi",
                        response?.Message ?? "Không thể tạo phiên điểm danh. Vui lòng thử lại.");
                }
            }
            catch (Exception ex)
            {
                await GetMetroWindow()?.ShowMessageAsync(
                    "Lỗi",
                    $"Đã xảy ra lỗi: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void NavigateBack()
        {
            _navigationService.GoBack();
        }
    }
}
