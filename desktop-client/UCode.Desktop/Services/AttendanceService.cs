using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UCode.Desktop.Models;

namespace UCode.Desktop.Services
{
    public class AttendanceService
    {
        private readonly ApiService _apiService;
        
        // Sample data storage (will be replaced by API calls)
        private readonly List<AttendanceSessionItem> _sampleSessions = new();
        private readonly List<AttendanceRecord> _sampleRecords = new();

        public AttendanceService(ApiService apiService)
        {
            _apiService = apiService;
            InitializeSampleData();
        }

        // TODO: Replace with actual API call
        public async Task<ApiResponse<List<AttendanceSessionItem>>> GetAttendanceSessionsAsync(string classId)
        {
            await Task.Delay(300); // Simulate API delay
            
            var sessions = _sampleSessions
                .Where(s => s.ClassId == classId)
                .OrderByDescending(s => s.CreatedAt)
                .ToList();

            return new ApiResponse<List<AttendanceSessionItem>>
            {
                Success = true,
                Data = sessions,
                Message = "Success"
            };
            
            // Real API call (uncomment when backend is ready):
            // return await _apiService.GetAsync<List<AttendanceSessionItem>>($"/api/v1/attendance/sessions?classId={classId}");
        }

        // TODO: Replace with actual API call
        public async Task<ApiResponse<AttendanceSessionItem>> CreateAttendanceSessionAsync(CreateAttendanceSessionRequest request)
        {
            await Task.Delay(300);
            
            var newSession = new AttendanceSessionItem
            {
                SessionId = Guid.NewGuid().ToString(),
                ClassId = request.ClassId,
                Title = request.Title,
                SessionCode = request.SessionCode,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                IsActive = request.IsActive,
                RequireIpCheck = request.RequireIpCheck,
                AllowedIpSubnet = request.AllowedIpSubnet,
                RequireGpsCheck = request.RequireGpsCheck,
                AllowedLatitude = request.AllowedLatitude,
                AllowedLongitude = request.AllowedLongitude,
                AllowedRadiusMeters = request.AllowedRadiusMeters,
                CreatedAt = DateTime.Now,
                AttendedCount = 0
            };
            
            _sampleSessions.Add(newSession);
            
            return new ApiResponse<AttendanceSessionItem>
            {
                Success = true,
                Data = newSession,
                Message = "Tạo phiên điểm danh thành công"
            };
            
            // Real API call:
            // return await _apiService.PostAsync<AttendanceSessionItem>("/api/v1/attendance/sessions", request);
        }

        // TODO: Replace with actual API call
        public async Task<ApiResponse<bool>> DeleteAttendanceSessionAsync(string sessionId)
        {
            await Task.Delay(200);
            
            var session = _sampleSessions.FirstOrDefault(s => s.SessionId == sessionId);
            if (session != null)
            {
                _sampleSessions.Remove(session);
                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Xóa phiên điểm danh thành công"
                };
            }
            
            return new ApiResponse<bool>
            {
                Success = false,
                Data = false,
                Message = "Không tìm thấy phiên điểm danh"
            };
            
            // Real API call:
            // return await _apiService.DeleteAsync($"/api/v1/attendance/sessions/{sessionId}");
        }

        // TODO: Replace with actual API call
        public async Task<ApiResponse<List<AttendanceRecord>>> GetAttendanceRecordsAsync(string sessionId)
        {
            await Task.Delay(300);
            
            // Generate sample records if not exists
            if (!_sampleRecords.Any(r => r.SessionId == sessionId))
            {
                GenerateSampleRecords(sessionId);
            }
            
            var records = _sampleRecords
                .Where(r => r.SessionId == sessionId)
                .OrderBy(r => r.AttendedAt)
                .ToList();

            return new ApiResponse<List<AttendanceRecord>>
            {
                Success = true,
                Data = records,
                Message = "Success"
            };
            
            // Real API call:
            // return await _apiService.GetAsync<List<AttendanceRecord>>($"/api/v1/attendance/sessions/{sessionId}/records");
        }

        private void GenerateSampleRecords(string sessionId)
        {
            var baseTime = DateTime.Now.AddDays(-7);
            var sampleRecords = new[]
            {
                new AttendanceRecord
                {
                    RecordId = Guid.NewGuid().ToString(),
                    SessionId = sessionId,
                    UserId = "user1",
                    StudentCode = "20120001",
                    FullName = "Nguyễn Văn An",
                    AttendedAt = baseTime.AddMinutes(5),
                    IpAddress = "192.168.1.100",
                    Latitude = 10.762622m,
                    Longitude = 106.660172m,
                    IsValid = true,
                    ValidationMessage = "Điểm danh hợp lệ"
                },
                new AttendanceRecord
                {
                    RecordId = Guid.NewGuid().ToString(),
                    SessionId = sessionId,
                    UserId = "user2",
                    StudentCode = "20120002",
                    FullName = "Trần Thị Bình",
                    AttendedAt = baseTime.AddMinutes(10),
                    IpAddress = "192.168.1.101",
                    Latitude = 10.762700m,
                    Longitude = 106.660200m,
                    IsValid = true,
                    ValidationMessage = "Điểm danh hợp lệ"
                },
                new AttendanceRecord
                {
                    RecordId = Guid.NewGuid().ToString(),
                    SessionId = sessionId,
                    UserId = "user3",
                    StudentCode = "20120003",
                    FullName = "Lê Văn Cường",
                    AttendedAt = baseTime.AddMinutes(15),
                    IpAddress = "10.0.0.50",
                    Latitude = 10.762622m,
                    Longitude = 106.660172m,
                    IsValid = false,
                    ValidationMessage = "IP không hợp lệ"
                },
                new AttendanceRecord
                {
                    RecordId = Guid.NewGuid().ToString(),
                    SessionId = sessionId,
                    UserId = "user4",
                    StudentCode = "20120004",
                    FullName = "Phạm Thị Dung",
                    AttendedAt = baseTime.AddMinutes(20),
                    IpAddress = "192.168.1.102",
                    Latitude = 10.763000m,
                    Longitude = 106.661000m,
                    IsValid = false,
                    ValidationMessage = "Vị trí GPS ngoài bán kính cho phép"
                },
                new AttendanceRecord
                {
                    RecordId = Guid.NewGuid().ToString(),
                    SessionId = sessionId,
                    UserId = "user5",
                    StudentCode = "20120005",
                    FullName = "Hoàng Văn Em",
                    AttendedAt = baseTime.AddMinutes(25),
                    IpAddress = "192.168.1.103",
                    Latitude = 10.762650m,
                    Longitude = 106.660180m,
                    IsValid = true,
                    ValidationMessage = "Điểm danh hợp lệ"
                },
                new AttendanceRecord
                {
                    RecordId = Guid.NewGuid().ToString(),
                    SessionId = sessionId,
                    UserId = "user6",
                    StudentCode = "20120006",
                    FullName = "Võ Thị Phương",
                    AttendedAt = baseTime.AddMinutes(30),
                    IpAddress = "192.168.1.104",
                    Latitude = 10.762600m,
                    Longitude = 106.660150m,
                    IsValid = true,
                    ValidationMessage = "Điểm danh hợp lệ"
                },
                new AttendanceRecord
                {
                    RecordId = Guid.NewGuid().ToString(),
                    SessionId = sessionId,
                    UserId = "user7",
                    StudentCode = "20120007",
                    FullName = "Đặng Văn Giang",
                    AttendedAt = baseTime.AddMinutes(35),
                    IpAddress = "192.168.1.105",
                    Latitude = 10.762680m,
                    Longitude = 106.660190m,
                    IsValid = true,
                    ValidationMessage = "Điểm danh hợp lệ"
                },
                new AttendanceRecord
                {
                    RecordId = Guid.NewGuid().ToString(),
                    SessionId = sessionId,
                    UserId = "user8",
                    StudentCode = "20120008",
                    FullName = "Bùi Thị Hà",
                    AttendedAt = baseTime.AddMinutes(40),
                    IpAddress = "192.168.1.106",
                    Latitude = 10.762640m,
                    Longitude = 106.660160m,
                    IsValid = true,
                    ValidationMessage = "Điểm danh hợp lệ"
                },
                new AttendanceRecord
                {
                    RecordId = Guid.NewGuid().ToString(),
                    SessionId = sessionId,
                    UserId = "user9",
                    StudentCode = "20120009",
                    FullName = "Ngô Văn Hùng",
                    AttendedAt = baseTime.AddMinutes(45),
                    IpAddress = "172.16.0.10",
                    Latitude = 10.762622m,
                    Longitude = 106.660172m,
                    IsValid = false,
                    ValidationMessage = "IP không nằm trong dải cho phép"
                },
                new AttendanceRecord
                {
                    RecordId = Guid.NewGuid().ToString(),
                    SessionId = sessionId,
                    UserId = "user10",
                    StudentCode = "20120010",
                    FullName = "Phan Thị Lan",
                    AttendedAt = baseTime.AddMinutes(50),
                    IpAddress = "192.168.1.107",
                    Latitude = 10.762610m,
                    Longitude = 106.660165m,
                    IsValid = true,
                    ValidationMessage = "Điểm danh hợp lệ"
                },
                new AttendanceRecord
                {
                    RecordId = Guid.NewGuid().ToString(),
                    SessionId = sessionId,
                    UserId = "user11",
                    StudentCode = "20120011",
                    FullName = "Trương Văn Minh",
                    AttendedAt = baseTime.AddMinutes(55),
                    IpAddress = "192.168.1.108",
                    Latitude = 10.762590m,
                    Longitude = 106.660140m,
                    IsValid = true,
                    ValidationMessage = "Điểm danh hợp lệ"
                },
                new AttendanceRecord
                {
                    RecordId = Guid.NewGuid().ToString(),
                    SessionId = sessionId,
                    UserId = "user12",
                    StudentCode = "20120012",
                    FullName = "Lý Thị Nga",
                    AttendedAt = baseTime.AddMinutes(60),
                    IpAddress = "192.168.1.109",
                    Latitude = 10.762655m,
                    Longitude = 106.660185m,
                    IsValid = true,
                    ValidationMessage = "Điểm danh hợp lệ"
                },
                new AttendanceRecord
                {
                    RecordId = Guid.NewGuid().ToString(),
                    SessionId = sessionId,
                    UserId = "user13",
                    StudentCode = "20120013",
                    FullName = "Đinh Văn Phúc",
                    AttendedAt = baseTime.AddMinutes(65),
                    IpAddress = "192.168.1.110",
                    Latitude = 10.764000m,
                    Longitude = 106.662000m,
                    IsValid = false,
                    ValidationMessage = "Vị trí GPS cách xa điểm điểm danh 250m"
                },
                new AttendanceRecord
                {
                    RecordId = Guid.NewGuid().ToString(),
                    SessionId = sessionId,
                    UserId = "user14",
                    StudentCode = "20120014",
                    FullName = "Vũ Thị Quỳnh",
                    AttendedAt = baseTime.AddMinutes(70),
                    IpAddress = "192.168.1.111",
                    Latitude = 10.762630m,
                    Longitude = 106.660175m,
                    IsValid = true,
                    ValidationMessage = "Điểm danh hợp lệ"
                },
                new AttendanceRecord
                {
                    RecordId = Guid.NewGuid().ToString(),
                    SessionId = sessionId,
                    UserId = "user15",
                    StudentCode = "20120015",
                    FullName = "Dương Văn Sơn",
                    AttendedAt = baseTime.AddMinutes(75),
                    IpAddress = "192.168.1.112",
                    Latitude = 10.762645m,
                    Longitude = 106.660168m,
                    IsValid = true,
                    ValidationMessage = "Điểm danh hợp lệ"
                }
            };

            _sampleRecords.AddRange(sampleRecords);
        }

        // TODO: Replace with actual API call
        public async Task<ApiResponse<AttendanceSessionItem>> UpdateAttendanceSessionAsync(string sessionId, UpdateAttendanceSessionRequest request)
        {
            await Task.Delay(300);
            
            var session = _sampleSessions.FirstOrDefault(s => s.SessionId == sessionId);
            if (session != null)
            {
                session.Title = request.Title ?? session.Title;
                session.StartTime = request.StartTime ?? session.StartTime;
                session.EndTime = request.EndTime ?? session.EndTime;
                session.IsActive = request.IsActive ?? session.IsActive;
                session.RequireIpCheck = request.RequireIpCheck ?? session.RequireIpCheck;
                session.AllowedIpSubnet = request.AllowedIpSubnet ?? session.AllowedIpSubnet;
                session.RequireGpsCheck = request.RequireGpsCheck ?? session.RequireGpsCheck;
                session.AllowedLatitude = request.AllowedLatitude ?? session.AllowedLatitude;
                session.AllowedLongitude = request.AllowedLongitude ?? session.AllowedLongitude;
                session.AllowedRadiusMeters = request.AllowedRadiusMeters ?? session.AllowedRadiusMeters;
                
                return new ApiResponse<AttendanceSessionItem>
                {
                    Success = true,
                    Data = session,
                    Message = "Cập nhật phiên điểm danh thành công"
                };
            }
            
            return new ApiResponse<AttendanceSessionItem>
            {
                Success = false,
                Message = "Không tìm thấy phiên điểm danh"
            };
            
            // Real API call:
            // return await _apiService.PutAsync<AttendanceSessionItem>($"/api/v1/attendance/sessions/{sessionId}", request);
        }

        private void InitializeSampleData()
        {
            // Sample sessions will be added per class when GetAttendanceSessionsAsync is called
            // This is just for demonstration - in real app, data comes from backend
        }

        public void AddSampleSessionsForClass(string classId)
        {
            // Check if already has sample data for this class
            if (_sampleSessions.Any(s => s.ClassId == classId))
                return;

            _sampleSessions.AddRange(new[]
            {
                new AttendanceSessionItem
                {
                    SessionId = Guid.NewGuid().ToString(),
                    ClassId = classId,
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
                },
                new AttendanceSessionItem
                {
                    SessionId = Guid.NewGuid().ToString(),
                    ClassId = classId,
                    Title = "Điểm danh tuần 2",
                    SessionCode = "WEEK02",
                    StartTime = DateTime.Now.AddHours(-1),
                    EndTime = DateTime.Now.AddHours(1),
                    IsActive = true,
                    AttendedCount = 32,
                    RequireIpCheck = false,
                    RequireGpsCheck = true,
                    AllowedLatitude = 10.762622m,
                    AllowedLongitude = 106.660172m,
                    AllowedRadiusMeters = 50,
                    CreatedAt = DateTime.Now.AddHours(-1)
                },
                new AttendanceSessionItem
                {
                    SessionId = Guid.NewGuid().ToString(),
                    ClassId = classId,
                    Title = "Điểm danh tuần 3",
                    SessionCode = "WEEK03",
                    StartTime = DateTime.Now.AddDays(7),
                    EndTime = DateTime.Now.AddDays(7).AddHours(2),
                    IsActive = false,
                    AttendedCount = 0,
                    RequireIpCheck = true,
                    AllowedIpSubnet = "192.168.1.0/24",
                    RequireGpsCheck = false,
                    CreatedAt = DateTime.Now
                }
            });
        }
    }

    public class CreateAttendanceSessionRequest
    {
        public string ClassId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string SessionCode { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsActive { get; set; }
        public bool RequireIpCheck { get; set; }
        public string? AllowedIpSubnet { get; set; }
        public bool RequireGpsCheck { get; set; }
        public decimal? AllowedLatitude { get; set; }
        public decimal? AllowedLongitude { get; set; }
        public int? AllowedRadiusMeters { get; set; }
    }

    public class UpdateAttendanceSessionRequest
    {
        public string? Title { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool? IsActive { get; set; }
        public bool? RequireIpCheck { get; set; }
        public string? AllowedIpSubnet { get; set; }
        public bool? RequireGpsCheck { get; set; }
        public decimal? AllowedLatitude { get; set; }
        public decimal? AllowedLongitude { get; set; }
        public int? AllowedRadiusMeters { get; set; }
    }

    public class AttendanceRecord
    {
        public string RecordId { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime AttendedAt { get; set; }
        public string? IpAddress { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public bool IsValid { get; set; }
        public string? ValidationMessage { get; set; }
    }
}
