using AutoMapper;
using UserService.Application.DTOs.Common;
using UserService.Application.DTOs.Requests;
using UserService.Application.DTOs.Responses;
using UserService.Application.Interfaces.Repositories;
using UserService.Application.Interfaces.Services;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IMapper _mapper;

    public AttendanceService(IAttendanceRepository attendanceRepository, IMapper mapper)
    {
        _attendanceRepository = attendanceRepository;
        _mapper = mapper;
    }

    private string GenerateRandomCode(int length = 6)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public async Task<ApiResponse<AttendanceRecordResponse>> CheckInAsync(AttendanceRecordRequest request)
    {
        try
        {
            // Create attendance record
            var attendanceRecord = _mapper.Map<AttendanceRecord>(request);
            attendanceRecord.Id = Guid.NewGuid();
            attendanceRecord.InvalidReason = string.Empty;
            
            // Validate session
            var session = await _attendanceRepository.GetSessionByCodeAsync(request.SessionCode);
            if (session == null || !session.IsActive || session.StartTime > DateTime.UtcNow || session.EndTime < DateTime.UtcNow)
            {
                return ApiResponse<AttendanceRecordResponse>.ErrorResponse("Session is not active or has expired");
            }

            // Validate IP address if required
            if (session.RequireIpCheck && !string.IsNullOrEmpty(session.AllowedIpSubnet))
            {
                if (!ValidateIpAddress(request.IpAddress, session.AllowedIpSubnet))
                {
                    attendanceRecord.IsValid = false;
                    attendanceRecord.InvalidReason += " IP không khớp ";
                }
            }

            // Validate GPS location if required
            if (session.RequireGpsCheck && session.AllowedLatitude.HasValue && session.AllowedLongitude.HasValue && session.AllowedRadiusMeters.HasValue)
            {
                var distance = CalculateDistance(
                    request.Latitude.GetValueOrDefault(),
                    request.Longitude.GetValueOrDefault(),
                    session.AllowedLatitude.Value,
                    session.AllowedLongitude.Value);

                if (distance > session.AllowedRadiusMeters.Value)
                {
                    attendanceRecord.IsValid = false;
                    attendanceRecord.InvalidReason += " Vị trí không khớp ";
                }
            }

            attendanceRecord.AttendedAt = DateTime.UtcNow;

            var record = await _attendanceRepository.CheckInAsync(attendanceRecord);
            var response = _mapper.Map<AttendanceRecordResponse>(record);

            return ApiResponse<AttendanceRecordResponse>.SuccessResponse(response, "Check-in completed successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<AttendanceRecordResponse>.ErrorResponse("An unexpected error occurred: " + ex.Message);
        }
    }

    public async Task<ApiResponse<AttendanceSessionResponse>> CreateSessionAsync(AttendanceSessionRequest request)
    {
        try
        {
            // Validate IP and GPS requirements
            if (request.RequireIpCheck && string.IsNullOrEmpty(request.AllowedIpSubnet))
            {
                return ApiResponse<AttendanceSessionResponse>.ErrorResponse("Allowed IP subnet is required when IP check is enabled");
            }

            if (request.RequireIpCheck && !IsValidIpv4(request.AllowedIpSubnet))
            {
                return ApiResponse<AttendanceSessionResponse>.ErrorResponse("Allowed IP subnet must be a valid IPv4 address or subnet");
            }

            if (request.RequireGpsCheck)
            {
                if (!request.AllowedLatitude.HasValue || !request.AllowedLongitude.HasValue || !request.AllowedRadiusMeters.HasValue)
                {
                    return ApiResponse<AttendanceSessionResponse>.ErrorResponse("GPS coordinates and radius are required when GPS check is enabled");
                }

                if (!IsValidLatitude(request.AllowedLatitude.Value) || !IsValidLongitude(request.AllowedLongitude.Value) || request.AllowedRadiusMeters.Value <= 0)
                {
                    return ApiResponse<AttendanceSessionResponse>.ErrorResponse("Invalid GPS coordinates or radius");
                }
            }

            var attendanceSession = _mapper.Map<AttendanceSession>(request);
            attendanceSession.Id = Guid.NewGuid();
            attendanceSession.CreatedAt = DateTime.UtcNow;
            attendanceSession.SessionCode = this.GenerateRandomCode();
            var session = await _attendanceRepository.CreateSessionAsync(attendanceSession);
            var response = _mapper.Map<AttendanceSessionResponse>(session);

            return ApiResponse<AttendanceSessionResponse>.SuccessResponse(response, "Session created successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<AttendanceSessionResponse>.ErrorResponse("An unexpected error occurred: " + ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> DeleteSessionAsync(Guid id)
    {
        try
        {
            var result = await _attendanceRepository.DeleteSessionAsync(id);
            if (!result)
                return ApiResponse<bool>.ErrorResponse("Session not found");

            return ApiResponse<bool>.SuccessResponse(true, "Session deleted successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.ErrorResponse("An unexpected error occurred: " + ex.Message);
        }
    }

    public async Task<ApiResponse<AttendanceRecordResponse?>> GetRecordBySessionAndUserAsync(Guid sessionId, Guid userId)
    {
        try
        {
            var record = await _attendanceRepository.GetRecordBySessionAndUserAsync(sessionId, userId);
            var response = _mapper.Map<AttendanceRecordResponse?>(record);
            return ApiResponse<AttendanceRecordResponse?>.SuccessResponse(response, "Record retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<AttendanceRecordResponse?>.ErrorResponse("An unexpected error occurred: " + ex.Message);
        }
    }

    public async Task<ApiResponse<List<AttendanceRecordResponse>>> GetRecordsBySessionAsync(Guid sessionId, int pageNumber, int pageSize)
    {
        try
        {
            var records = await _attendanceRepository.GetRecordsBySessionAsync(sessionId, pageNumber, pageSize);
            var response = _mapper.Map<List<AttendanceRecordResponse>>(records);
            return ApiResponse<List<AttendanceRecordResponse>>.SuccessResponse(response, "Records retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<AttendanceRecordResponse>>.ErrorResponse("An unexpected error occurred: " + ex.Message);
        }
    }

    public async Task<ApiResponse<AttendanceSessionResponse?>> GetSessionByIdAsync(Guid id)
    {
        try
        {
            var session = await _attendanceRepository.GetSessionByIdAsync(id);
            if (session == null)
                return ApiResponse<AttendanceSessionResponse?>.ErrorResponse("Session not found");

            var response = _mapper.Map<AttendanceSessionResponse?>(session);
            return ApiResponse<AttendanceSessionResponse?>.SuccessResponse(response, "Session retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<AttendanceSessionResponse?>.ErrorResponse("An unexpected error occurred: " + ex.Message);
        }
    }

    public async Task<ApiResponse<List<AttendanceSessionResponse>>> GetSessionsAsync(Guid classId, int pageNumber, int pageSize)
    {
        try
        {
            var sessions = await _attendanceRepository.GetSessionsAsync(classId, pageNumber, pageSize);
            var response = _mapper.Map<List<AttendanceSessionResponse>>(sessions);
            return ApiResponse<List<AttendanceSessionResponse>>.SuccessResponse(response, "Sessions retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<AttendanceSessionResponse>>.ErrorResponse("An unexpected error occurred: " + ex.Message);
        }
    }

    public async Task<ApiResponse<int>> GetTotalRecordsBySessionAsync(Guid sessionId)
    {
        try
        {
            var totalRecords = await _attendanceRepository.GetTotalRecordsBySessionAsync(sessionId);
            return ApiResponse<int>.SuccessResponse(totalRecords, "Total records retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<int>.ErrorResponse("An unexpected error occurred: " + ex.Message);
        }
    }

    public async Task<ApiResponse<AttendanceSessionResponse>> UpdateSessionAsync(AttendanceSessionRequest attendanceSession)
    {
        try
        {
            if (attendanceSession.Id == null)
                return ApiResponse<AttendanceSessionResponse>.ErrorResponse("Session ID is required");
            var existingSession = await _attendanceRepository.GetSessionByIdAsync(attendanceSession.Id.Value);
            if (existingSession == null)
                return ApiResponse<AttendanceSessionResponse>.ErrorResponse("Session not found");

            // Validate IP and GPS requirements
            if (attendanceSession.RequireIpCheck && string.IsNullOrEmpty(attendanceSession.AllowedIpSubnet))
            {
                return ApiResponse<AttendanceSessionResponse>.ErrorResponse("Allowed IP subnet is required when IP check is enabled");
            }

            if (attendanceSession.RequireIpCheck && !IsValidIpv4(attendanceSession.AllowedIpSubnet))
            {
                return ApiResponse<AttendanceSessionResponse>.ErrorResponse("Allowed IP subnet must be a valid IPv4 address or subnet");
            }

            if (attendanceSession.RequireGpsCheck)
            {
                if (!attendanceSession.AllowedLatitude.HasValue || !attendanceSession.AllowedLongitude.HasValue || !attendanceSession.AllowedRadiusMeters.HasValue)
                {
                    return ApiResponse<AttendanceSessionResponse>.ErrorResponse("GPS coordinates and radius are required when GPS check is enabled");
                }

                if (!IsValidLatitude(attendanceSession.AllowedLatitude.Value) || !IsValidLongitude(attendanceSession.AllowedLongitude.Value) || attendanceSession.AllowedRadiusMeters.Value <= 0)
                {
                    return ApiResponse<AttendanceSessionResponse>.ErrorResponse("Invalid GPS coordinates or radius");
                }
            }

            var updatedSession = _mapper.Map<AttendanceSession>(attendanceSession);
            updatedSession.CreatedAt = existingSession.CreatedAt;
            updatedSession.Id = existingSession.Id; 
            updatedSession.SessionCode = existingSession.SessionCode; 
            updatedSession.ClassId = existingSession.ClassId;

            var session = await _attendanceRepository.UpdateSessionAsync(updatedSession);
            var response = _mapper.Map<AttendanceSessionResponse>(session);

            return ApiResponse<AttendanceSessionResponse>.SuccessResponse(response, "Session updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<AttendanceSessionResponse>.ErrorResponse("An unexpected error occurred: " + ex.Message);
        }
    }

    private bool ValidateIpAddress(string? ipAddress, string allowedSubnet)
    {
        // Logic to validate if the IP address is within the allowed subnet
        // This is a placeholder and should be replaced with actual subnet validation logic
        return ipAddress == allowedSubnet;
    }

    private bool IsValidIpv4(string? ip)
    {
        if (string.IsNullOrEmpty(ip)) return false;
        // Simple IPv4 regex (supports subnet like 192.168.1.0/24)
        var ipv4Regex = @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(?:\/(?:[0-9]|[1-2][0-9]|3[0-2]))?$";
        return System.Text.RegularExpressions.Regex.IsMatch(ip, ipv4Regex);
    }

    private bool IsValidLatitude(decimal latitude)
    {
        return latitude >= -90 && latitude <= 90;
    }

    private bool IsValidLongitude(decimal longitude)
    {
        return longitude >= -180 && longitude <= 180;
    }

    private double CalculateDistance(decimal latitude1, decimal longitude1, decimal latitude2, decimal longitude2)
    {
        const double EarthRadius = 6371e3; // meters
        var lat1Rad = (double)latitude1 * Math.PI / 180;
        var lat2Rad = (double)latitude2 * Math.PI / 180;
        var deltaLat = ((double)latitude2 - (double)latitude1) * Math.PI / 180;
        var deltaLon = ((double)longitude2 - (double)longitude1) * Math.PI / 180;

        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadius * c;
    }

    public async Task<ApiResponse<AttendanceSessionResponse?>> GetSessionByCodeAsync(string code)
    {
        try
        {
            var session = await _attendanceRepository.GetSessionByCodeAsync(code);
            if (session == null)
                return ApiResponse<AttendanceSessionResponse?>.ErrorResponse("Session not found");

            var response = _mapper.Map<AttendanceSessionResponse?>(session);
            return ApiResponse<AttendanceSessionResponse?>.SuccessResponse(response, "Session retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<AttendanceSessionResponse?>.ErrorResponse("An unexpected error occurred: " + ex.Message);
        }
    }

    public async Task<ApiResponse<AttendanceRecordResponse>> CheckAttendanceStatusAsync(Guid sessionId, Guid userId)
    {
        try
        {
            var record = await _attendanceRepository.GetRecordBySessionAndUserAsync(sessionId, userId);
            if (record == null)
                return ApiResponse<AttendanceRecordResponse>.ErrorResponse("Session not found");

            var response = _mapper.Map<AttendanceRecordResponse>(record);

            return ApiResponse<AttendanceRecordResponse>.SuccessResponse(response, "Attendance status retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<AttendanceRecordResponse>.ErrorResponse("An unexpected error occurred: " + ex.Message);
        }
    }
}