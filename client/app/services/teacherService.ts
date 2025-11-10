import { API } from '../api'
import type {
  ApiResponse,
  PagedResponse,
  TeacherResponse,
  CreateTeacherRequest,
  UpdateTeacherRequest,
  CreateAdminRequest,
  Class,
} from '../types'
import { handleApiError, unwrapApiResponse, buildQueryString } from './utils'

// ==================== REQUEST TYPES ====================

export interface GetTeachersParams {
  pageNumber?: number
  pageSize?: number
  search?: string
}

// ==================== TEACHER SERVICE ====================

/**
 * Get current teacher profile (self-service)
 */
export async function getMyProfile(): Promise<TeacherResponse> {
  try {
    const response = await API.get<ApiResponse<TeacherResponse>>('api/v1/teachers/me')
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Update current teacher profile (self-service)
 */
export async function updateMyProfile(data: UpdateTeacherRequest): Promise<TeacherResponse> {
  try {
    const response = await API.put<ApiResponse<TeacherResponse>>('api/v1/teachers/me', data)
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Get my classes (self-service)
 */
export async function getMyClasses(): Promise<Class[]> {
  try {
    const response = await API.get<ApiResponse<Class[]>>('api/v1/teachers/me/classes')
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Get all teachers with pagination and search (Admin)
 */
export async function getAllTeachers(
  params?: GetTeachersParams,
): Promise<PagedResponse<TeacherResponse>> {
  try {
    const queryString = buildQueryString(params || {})
    const response = await API.get<ApiResponse<PagedResponse<TeacherResponse>>>(
      `api/v1/teachers${queryString}`,
    )
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Get teacher by ID (Admin)
 */
export async function getTeacherById(id: string): Promise<TeacherResponse> {
  try {
    const response = await API.get<ApiResponse<TeacherResponse>>(`api/v1/teachers/${id}`)
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Get teacher by teacher code (Admin)
 */
export async function getTeacherByCode(teacherCode: string): Promise<TeacherResponse> {
  try {
    const response = await API.get<ApiResponse<TeacherResponse>>(
      `api/v1/teachers/by-teacher-code/${teacherCode}`,
    )
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Get classes taught by a specific teacher (Admin)
 */
export async function getTeacherClasses(teacherId: string): Promise<Class[]> {
  try {
    const response = await API.get<ApiResponse<Class[]>>(`api/v1/teachers/${teacherId}/classes`)
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Create a new teacher (Admin)
 */
export async function createTeacher(data: CreateTeacherRequest): Promise<TeacherResponse> {
  try {
    const response = await API.post<ApiResponse<TeacherResponse>>('api/v1/teachers/create', data)
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Create a new admin user (Admin only)
 */
export async function createAdmin(data: CreateAdminRequest): Promise<TeacherResponse> {
  try {
    const response = await API.post<ApiResponse<TeacherResponse>>(
      'api/v1/teachers/create-admin',
      data,
    )
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Update teacher by ID (Admin)
 */
export async function updateTeacher(
  id: string,
  data: UpdateTeacherRequest,
): Promise<TeacherResponse> {
  try {
    const response = await API.put<ApiResponse<TeacherResponse>>(`api/v1/teachers/${id}`, data)
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Delete teacher by ID (Admin)
 */
export async function deleteTeacher(id: string): Promise<void> {
  try {
    const response = await API.delete<ApiResponse<object>>(`api/v1/teachers/${id}`)
    unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}
