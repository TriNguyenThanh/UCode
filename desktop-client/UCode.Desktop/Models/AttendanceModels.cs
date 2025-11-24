using System;
using Newtonsoft.Json;

namespace UCode.Desktop.Models
{
    public class AttendanceSessionItem
    {
        [JsonProperty("id")]
        public string SessionId { get; set; } = Guid.NewGuid().ToString();
        public string ClassId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string SessionCode { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool RequireIpCheck { get; set; }
        public string? AllowedIpSubnet { get; set; }
        public bool RequireGpsCheck { get; set; }
        public decimal? AllowedLatitude { get; set; }
        public decimal? AllowedLongitude { get; set; }
        public int? AllowedRadiusMeters { get; set; }
        public bool IsActive { get; set; }
        public int AttendedCount { get; set; }
        public DateTime CreatedAt { get; set; }

        public string StatusText => IsActive ? "Đang mở" : "Đã đóng";
    }

    public class AttendanceRecordItem
    {
        [JsonProperty("id")]
        public string RecordId { get; set; } = Guid.NewGuid().ToString();
        public string SessionId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime AttendedAt { get; set; }
        public string? IpAddress { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? UserAgent { get; set; }
        public bool IsValid { get; set; }
        public string? InvalidReason { get; set; }

        public string StatusText => IsValid ? "Hợp lệ" : "Không hợp lệ";
    }
}


