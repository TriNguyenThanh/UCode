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
        }

        public async Task<ApiResponse<AttendanceSessionItem>> GetAttendanceSessionByIdAsync(string sessionId)
        {
            return await _apiService.GetAsync<AttendanceSessionItem>($"/api/v1/attendance/session/{sessionId}");
        }

        public async Task<ApiResponse<List<AttendanceSessionItem>>> GetAttendanceSessionsAsync(string classId)
        {
            return await _apiService.GetAsync<List<AttendanceSessionItem>>($"/api/v1/attendance/sessions?classId={classId}");
        }

        public async Task<ApiResponse<AttendanceSessionItem>> CreateAttendanceSessionAsync(CreateAttendanceSessionRequest request)
        {           
            return await _apiService.PostAsync<AttendanceSessionItem>("/api/v1/attendance/create-session", request);
        }

        public async Task<ApiResponse<bool>> DeleteAttendanceSessionAsync(string sessionId)
        {
            return await _apiService.DeleteAsync($"/api/v1/attendance/sessions/{sessionId}");
        }

        public async Task<ApiResponse<List<AttendanceRecord>>> GetAttendanceRecordsAsync(string sessionId)
        {
           
            return await _apiService.GetAsync<List<AttendanceRecord>>($"/api/v1/attendance/records/by-session/{sessionId}");
        }

        public async Task<ApiResponse<AttendanceSessionItem>> UpdateAttendanceSessionAsync(string sessionId, UpdateAttendanceSessionRequest request)
        {
            request.Id = sessionId;
            return await _apiService.PutAsync<AttendanceSessionItem>($"/api/v1/attendance/session", request);
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
        public string Id { get; set; }
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
        public DateTime? AttendedAt { get; set; }
        public string? IpAddress { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public bool IsValid { get; set; }
        public string? ValidationMessage { get; set; }
    }
}
