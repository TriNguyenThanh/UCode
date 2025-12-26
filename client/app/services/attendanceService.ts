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
      ipAddress: request.ipAddress, // Send IP address for testing
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

// ============================================
// TEACHER APIs
// ============================================

export interface CreateAttendanceSessionRequest {
  classId: string
  title: string
  sessionCode: string
  startTime: string // ISO string
  endTime: string // ISO string
  requireIpCheck: boolean
  allowedIpSubnet?: string
  requireGpsCheck: boolean
  allowedLatitude?: number
  allowedLongitude?: number
  allowedRadiusMeters?: number
  requireFaceCheck: boolean
  isActive: boolean
}

export interface UpdateAttendanceSessionRequest extends CreateAttendanceSessionRequest {
  id: string
}

/**
 * [Teacher] Tạo phiên điểm danh mới
 * POST /api/v1/attendance/create-session
 */
export async function createAttendanceSession(
  request: CreateAttendanceSessionRequest
): Promise<AttendanceSession> {
  const response = await API.post<ApiResponse<AttendanceSession>>(
    '/api/v1/attendance/create-session',
    request
  )
  if (!response.data.success || !response.data.data) {
    throw new Error(response.data.message || 'Không thể tạo phiên điểm danh')
  }
  return response.data.data
}

/**
 * [Teacher] Cập nhật phiên điểm danh
 * PUT /api/v1/attendance/update-session/{id}
 */
export async function updateAttendanceSession(
  id: string,
  request: Omit<UpdateAttendanceSessionRequest, 'id'>
): Promise<AttendanceSession> {
  const response = await API.put<ApiResponse<AttendanceSession>>(
    `/api/v1/attendance/session`,
    { ...request, id }
  )
  if (!response.data.success || !response.data.data) {
    throw new Error(response.data.message || 'Không thể cập nhật phiên điểm danh')
  }
  return response.data.data
}

/**
 * [Teacher] Lấy danh sách phiên điểm danh của lớp
 * GET /api/v1/attendance/sessions?classId
 */
export async function getAttendanceSessions(
  classId: string,
  pageNumber: number = 1,
  pageSize: number = 10
): Promise<AttendanceSession[]> {
  const response = await API.get<ApiResponse<AttendanceSession[]>>(
    `/api/v1/attendance/sessions?classId=${classId}&pageNumber=${pageNumber}&pageSize=${pageSize}`
  )
  if (!response.data.success || !response.data.data) {
    throw new Error(response.data.message || 'Không thể tải danh sách phiên điểm danh')
  }
  return response.data.data
}

/**
 * [Teacher] Lấy thông tin phiên điểm danh theo ID
 * GET /api/v1/attendance/session/{id}
 */
export async function getAttendanceSessionById(
  sessionId: string
): Promise<AttendanceSession> {
  const response = await API.get<ApiResponse<AttendanceSession>>(
    `/api/v1/attendance/session/${sessionId}`
  )
  if (!response.data.success || !response.data.data) {
    throw new Error(response.data.message || 'Không thể tải thông tin phiên điểm danh')
  }
  return response.data.data
}

/**
 * [Teacher] Xóa phiên điểm danh
 * DELETE /api/v1/attendance/delete-session/{id}
 */
export async function deleteAttendanceSession(id: string): Promise<void> {
  const response = await API.delete<ApiResponse<void>>(
    `/api/v1/attendance/delete-session/${id}`
  )
  if (!response.data.success) {
    throw new Error(response.data.message || 'Không thể xóa phiên điểm danh')
  }
}
