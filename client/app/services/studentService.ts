import { API } from '../api'
import type {
  ApiResponse,
  PagedResponse,
  StudentResponse,
  CreateStudentRequest,
  UpdateStudentRequest,
} from '../types'
import { handleApiError, unwrapApiResponse, buildQueryString } from './utils'

// ==================== REQUEST TYPES ====================

export interface GetStudentsParams {
  pageNumber?: number
  pageSize?: number
  search?: string
  classId?: string
}

export interface ImportStudentsRequest {
  students: CreateStudentRequest[]
}

// ==================== STUDENT SERVICE ====================

/**
 * Get current student profile (self-service)
 */
export async function getMyProfile(): Promise<StudentResponse> {
  try {
    const response = await API.get<ApiResponse<StudentResponse>>('api/v1/students/me')
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Update current student profile (self-service)
 */
export async function updateMyProfile(data: UpdateStudentRequest): Promise<StudentResponse> {
  try {
    const response = await API.put<ApiResponse<StudentResponse>>('api/v1/students/me', data)
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Get all students with pagination and search (Admin/Teacher)
 */
export async function getAllStudents(
  params?: GetStudentsParams,
): Promise<PagedResponse<StudentResponse>> {
  try {
    const queryString = buildQueryString(params || {})
    const response = await API.get<ApiResponse<PagedResponse<StudentResponse>>>(
      `api/v1/students${queryString}`,
    )
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Get student by ID (Admin/Teacher)
 */
export async function getStudentById(id: string): Promise<StudentResponse> {
  try {
    const response = await API.get<ApiResponse<StudentResponse>>(`api/v1/students/${id}`)
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Get student by student code (Admin/Teacher)
 */
export async function getStudentByCode(studentCode: string): Promise<StudentResponse> {
  try {
    const response = await API.get<ApiResponse<StudentResponse>>(
      `api/v1/students/by-student-code/${studentCode}`,
    )
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Create a new student (Admin/Teacher)
 */
export async function createStudent(data: CreateStudentRequest): Promise<StudentResponse> {
  try {
    const response = await API.post<ApiResponse<StudentResponse>>('api/v1/students/create', data)
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Update student by ID (Admin/Teacher)
 */
export async function updateStudent(
  id: string,
  data: UpdateStudentRequest,
): Promise<StudentResponse> {
  try {
    const response = await API.put<ApiResponse<StudentResponse>>(`api/v1/students/${id}`, data)
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Delete student by ID (Admin)
 */
export async function deleteStudent(id: string): Promise<void> {
  try {
    const response = await API.delete<ApiResponse<object>>(`api/v1/students/${id}`)
    unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Import students from Excel file (Admin/Teacher)
 */
export async function importStudentsExcel(
  file: File,
): Promise<{ successCount: number; errors: string[] }> {
  try {
    const formData = new FormData()
    formData.append('file', file)

    const response = await API.post<ApiResponse<{ successCount: number; errors: string[] }>>(
      'api/v1/students/import-excel',
      formData,
      {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      },
    )
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Export students to Excel file (Admin/Teacher)
 */
export async function exportStudentsExcel(studentIds?: string[]): Promise<Blob> {
  try {
    const params = studentIds?.length ? { studentIds: studentIds.join(',') } : {}
    const queryString = buildQueryString(params)
    const response = await API.get(`api/v1/students/export-excel${queryString}`, {
      responseType: 'blob',
    })
    return response.data
  } catch (error) {
    handleApiError(error)
  }
}
