import { API } from '~/api'
import type {
  ApiResponse,
  AttendanceSession,
  AttendanceRecord,
  AttendanceCheckInRequest,
} from '~/types'

/**
 * Lấy thông tin phiên điểm danh theo session code
 * GET /api/v1/attendance/session-by-code/{code}
 */
export async function getAttendanceSessionByCode(
  sessionCode: string
): Promise<AttendanceSession> {
  const response = await API.get<ApiResponse<AttendanceSession>>(
    `/api/v1/attendance/session-by-code/${sessionCode}`
  )
  if (!response.data.success || !response.data.data) {
    throw new Error(response.data.message || 'Không thể tải thông tin phiên điểm danh')
  }
  return response.data.data
}

/**
 * Sinh viên điểm danh
 * POST /api/v1/attendance/records/check-in
 */
export async function checkInAttendance(
  request: AttendanceCheckInRequest
): Promise<AttendanceRecord> {
  const response = await API.post<ApiResponse<AttendanceRecord>>(
    '/api/v1/attendance/records/check-in',
    {
      sessionCode: request.sessionCode,
      sessionId: request.sessionId,
      latitude: request.latitude,
      longitude: request.longitude,
    }
  )
  if (!response.data.success || !response.data.data) {
    throw new Error(response.data.message || 'Điểm danh thất bại')
  }
  return response.data.data
}

/**
 * Kiểm tra sinh viên đã điểm danh chưa
 * GET /api/v1/attendance/session/{sessionId}/status
 */
export async function checkAttendanceStatus(
  sessionId: string
): Promise<AttendanceRecord | null> {
  try {
    const response = await API.get<ApiResponse<AttendanceRecord>>(
      `/api/v1/attendance/session/${sessionId}/status`
    )
    if (response.data.success && response.data.data) {
      return response.data.data
    }
    return null
  } catch (error) {
    return null
  }
}
