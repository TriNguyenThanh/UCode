// import { API } from '~/api'
import type {
  // ApiResponse,
  AttendanceSession,
  AttendanceRecord,
  AttendanceCheckInRequest,
} from '~/types'

// Mock data store
const mockAttendanceRecords = new Map<string, AttendanceRecord>()

/**
 * Lấy thông tin phiên điểm danh theo session code
 */
export async function getAttendanceSessionByCode(
  sessionCode: string
): Promise<AttendanceSession> {
  // TODO: Uncomment when backend API is ready
  // const response = await API.get<ApiResponse<AttendanceSession>>(
  //   `/api/attendance/session/${sessionCode}`
  // )
  // if (!response.data.success || !response.data.data) {
  //   throw new Error(response.data.message || 'Failed to fetch attendance session')
  // }
  // return response.data.data

  // Mock data
  await new Promise(resolve => setTimeout(resolve, 500)) // Simulate network delay
  
  if (sessionCode === 'INVALID') {
    throw new Error('Không tìm thấy phiên điểm danh')
  }

  return {
    id: '1',
    classId: 'class-1',
    className: 'Lập trình Web - Nhóm 01',
    title: 'Điểm danh buổi 5 - React Hooks',
    sessionCode: sessionCode,
    startTime: new Date(Date.now() - 30 * 60 * 1000).toISOString(), // 30 phút trước
    endTime: new Date(Date.now() + 30 * 60 * 1000).toISOString(), // 30 phút sau
    requireIpCheck: false,
    requireGpsCheck: true,
    isActive: true,
    createdAt: new Date(Date.now() - 60 * 60 * 1000).toISOString()
  }
}

/**
 * Sinh viên điểm danh
 */
export async function checkInAttendance(
  request: AttendanceCheckInRequest
): Promise<AttendanceRecord> {
  // TODO: Uncomment when backend API is ready
  // const response = await API.post<ApiResponse<AttendanceRecord>>(
  //   '/api/attendance/check-in',
  //   request
  // )
  // if (!response.data.success || !response.data.data) {
  //   throw new Error(response.data.message || 'Failed to check in')
  // }
  // return response.data.data

  // Mock data
  await new Promise(resolve => setTimeout(resolve, 800)) // Simulate network delay

  const userId = localStorage.getItem('userId') || 'user-1'
  const recordKey = `${request.sessionCode}-${userId}`

  // Kiểm tra đã điểm danh chưa
  if (mockAttendanceRecords.has(recordKey)) {
    throw new Error('Bạn đã điểm danh cho phiên này rồi')
  }

  const record: AttendanceRecord = {
    id: `record-${Date.now()}`,
    sessionId: 'session-1',
    userId: userId,
    attendedAt: new Date().toISOString(),
    ipAddress: '192.168.1.100',
    latitude: request.latitude,
    longitude: request.longitude,
    userAgent: 'Mock User Agent',
    isValid: true
  }

  mockAttendanceRecords.set(recordKey, record)
  return record
}

/**
 * Kiểm tra sinh viên đã điểm danh chưa
 */
export async function checkAttendanceStatus(
  sessionCode: string
): Promise<AttendanceRecord | null> {
  // TODO: Uncomment when backend API is ready
  // try {
  //   const response = await API.get<ApiResponse<AttendanceRecord>>(
  //     `/api/attendance/session/${sessionCode}/status`
  //   )
  //   if (response.data.success && response.data.data) {
  //     return response.data.data
  //   }
  //   return null
  // } catch (error) {
  //   return null
  // }

  // Mock data
  await new Promise(resolve => setTimeout(resolve, 300)) // Simulate network delay

  const userId = localStorage.getItem('userId') || 'user-1'
  const recordKey = `${sessionCode}-${userId}`

  return mockAttendanceRecords.get(recordKey) || null
}
