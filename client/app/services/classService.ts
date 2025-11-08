import { API } from '../api'
import type { ApiResponse, Class, ClassWithStudents, User } from '../types'
import { handleApiError, unwrapApiResponse } from './utils'

// ==================== REQUEST TYPES ====================

export interface CreateClassRequest {
  className: string
  classCode: string
  teacherId: string
  semester: string
  description?: string
  coverImage?: string
}

export interface UpdateClassRequest {
  classId: string
  className?: string
  description?: string
  coverImage?: string
}

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
 * Creates a new class (Teacher only)
 */
export async function createClass(data: CreateClassRequest): Promise<Class> {
  try {
    const response = await API.post<ApiResponse<Class>>('/api/v1/classes/create', data)
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Gets a class by ID
 */
export async function getClass(classId: string): Promise<Class> {
  try {
    const response = await API.get<ApiResponse<Class>>(`/api/v1/classes/${classId}`)
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Gets class detail including student list
 */
export async function getClassDetail(classId: string): Promise<ClassWithStudents> {
  try {
    const response = await API.get<ApiResponse<ClassWithStudents>>(
      `/api/v1/classes/${classId}/detail`
    )
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Gets a list of classes with pagination and filters
 */
export async function getClasses(params?: {
  pageNumber?: number
  pageSize?: number
  teacherId?: string
  isActive?: boolean
}): Promise<Class[]> {
  try {
    const queryParams = new URLSearchParams()
    if (params?.pageNumber) queryParams.append('pageNumber', params.pageNumber.toString())
    if (params?.pageSize) queryParams.append('pageSize', params.pageSize.toString())
    if (params?.teacherId) queryParams.append('teacherId', params.teacherId)
    if (params?.isActive !== undefined) queryParams.append('isActive', params.isActive.toString())

    const response = await API.get<ApiResponse<Class[]>>(
      `/api/v1/classes?${queryParams.toString()}`
    )
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Updates a class (Teacher only)
 */
export async function updateClass(data: UpdateClassRequest): Promise<void> {
  try {
    const response = await API.put<ApiResponse<object>>('/api/v1/classes/update', data)
    unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Deletes a class (Teacher only)
 */
export async function deleteClass(classId: string): Promise<void> {
  try {
    const response = await API.delete<ApiResponse<object>>(`/api/v1/classes/delete?id=${classId}`)
    unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Adds a student to a class
 */
export async function addStudentToClass(data: AddStudentToClassRequest): Promise<void> {
  try {
    const response = await API.post<ApiResponse<object>>('/api/v1/classes/add-student', data)
    unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Adds multiple students to a class
 */
export async function addStudentsToClass(data: AddStudentsToClassRequest): Promise<void> {
  try {
    const response = await API.post<ApiResponse<object>>('/api/v1/classes/add-students', data)
    unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Removes a student from a class
 */
export async function removeStudentFromClass(
  classId: string,
  studentId: string
): Promise<void> {
  try {
    const response = await API.delete<ApiResponse<object>>(
      `/api/v1/classes/remove-student?classId=${classId}&studentId=${studentId}`
    )
    unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Gets list of students in a class
 */
export async function getStudentsInClass(classId: string): Promise<User[]> {
  try {
    const response = await API.get<ApiResponse<User[]>>(`/api/v1/classes/${classId}/students`)
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}
