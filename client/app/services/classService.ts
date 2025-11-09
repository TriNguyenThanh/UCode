import { API } from '../api'
import type {
  ApiResponse,
  PagedResponse,
  Class,
  CreateClassRequest,
  UpdateClassRequest,
  GetClassesRequest,
  StudentResponse,
} from '../types'
import { handleApiError, unwrapApiResponse, buildQueryString } from './utils'

// ==================== REQUEST TYPES ====================

export interface AddStudentToClassRequest {
  classId: string
  studentId: string
}

export interface AddStudentsToClassRequest {
  classId: string
  studentIds: string[]
}

// ==================== CLASS SERVICE ====================

/**
 * Get all classes with pagination and filters (Admin/Teacher)
 * Backend: GET /api/v1/classes?pageNumber=1&pageSize=10&teacherId=xxx&isActive=true
 */
export async function getAllClasses(
  params?: GetClassesRequest,
): Promise<PagedResponse<Class>> {
  try {
    const queryString = buildQueryString(params || {})
    const response = await API.get<ApiResponse<PagedResponse<Class>>>(
      `api/v1/classes${queryString}`,
    )
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Get class by ID
 * Backend: GET /api/v1/classes/{id}
 */
export async function getClassById(id: string): Promise<Class> {
  try {
    const response = await API.get<ApiResponse<Class>>(`api/v1/classes/${id}`)
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Get class detail with students
 * Backend: GET /api/v1/classes/{id}/detail
 */
export async function getClassDetail(id: string): Promise<Class> {
  try {
    const response = await API.get<ApiResponse<Class>>(`api/v1/classes/${id}/detail`)
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Get enrolled classes (Student)
 * TODO: Backend chưa có endpoint này
 * Temporary: Use GET /api/v1/classes with current student filter
 */
export async function getEnrolledClasses(): Promise<Class[]> {
  try {
    // TODO: Backend cần implement GET /api/v1/classes/enrolled
    // Tạm thời return empty array
    console.warn('Backend endpoint /api/v1/classes/enrolled chưa được implement')
    return []
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Get students in a class
 * Backend: GET /api/v1/classes/{classId}/students
 */
export async function getClassStudents(classId: string): Promise<StudentResponse[]> {
  try {
    const response = await API.get<ApiResponse<StudentResponse[]>>(
      `api/v1/classes/${classId}/students`,
    )
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Create a new class (Teacher/Admin)
 * Backend: POST /api/v1/classes/create
 */
export async function createClass(data: CreateClassRequest): Promise<Class> {
  try {
    const response = await API.post<ApiResponse<Class>>('api/v1/classes/create', data)
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Update class
 * Backend: PUT /api/v1/classes/update
 */
export async function updateClass(data: UpdateClassRequest): Promise<void> {
  try {
    const response = await API.put<ApiResponse<void>>('api/v1/classes/update', data)
    unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Delete class by ID (Teacher/Admin)
 * Backend: DELETE /api/v1/classes/delete?id={id}
 */
export async function deleteClass(id: string): Promise<void> {
  try {
    const response = await API.delete<ApiResponse<void>>(`api/v1/classes/delete?id=${id}`)
    unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Add a student to a class (Teacher/Admin)
 * Backend: POST /api/v1/classes/add-student
 */
export async function addStudentToClass(classId: string, studentId: string): Promise<void> {
  try {
    const response = await API.post<ApiResponse<void>>(
      'api/v1/classes/add-student',
      { classId, studentId }
    )
    unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Remove a student from a class (Teacher/Admin)
 * Backend: DELETE /api/v1/classes/remove-student?classId={classId}&studentId={studentId}
 */
export async function removeStudentFromClass(classId: string, studentId: string): Promise<void> {
  try {
    const response = await API.delete<ApiResponse<void>>(
      `api/v1/classes/remove-student?classId=${classId}&studentId=${studentId}`,
    )
    unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Bulk add students to a class (Teacher/Admin)
 * Backend: POST /api/v1/classes/add-students
 */
export async function addStudentsToClass(classId: string, studentIds: string[]): Promise<void> {
  try {
    const response = await API.post<ApiResponse<void>>(
      'api/v1/classes/add-students',
      { classId, studentIds }
    )
    unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}
