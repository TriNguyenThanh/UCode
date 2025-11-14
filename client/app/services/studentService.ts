import { API } from '../api'
import type {
  ApiResponse,
  PagedResponse,
  BackendPagedResponse,
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
    const response = await API.get<ApiResponse<BackendPagedResponse<StudentResponse>>>(
      `api/v1/students${queryString}`,
    )
    
    // Backend wraps BackendPagedResponse inside ApiResponse
    const result = unwrapApiResponse(response.data)
    
    return {
      data: result.items || [],
      page: result.pageNumber,
      pageSize: result.pageSize,
      totalCount: result.totalCount,
      totalPages: result.totalPages,
      hasPrevious: result.hasPreviousPage || false,
      hasNext: result.hasNextPage || false,
    }
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
 * Bulk create multiple students at once (Admin/Teacher - optimized)
 */
export interface BulkCreateResult {
  successCount: number
  failureCount: number
  results: BulkCreateStudentResult[]
}

export interface BulkCreateStudentResult {
  studentCode: string
  success: boolean
  errorMessage?: string
  userId?: string
}

export async function bulkCreateStudents(
  students: CreateStudentRequest[],
): Promise<BulkCreateResult> {
  try {
    const response = await API.post<ApiResponse<BulkCreateResult>>(
      'api/v1/students/bulk-create',
      { students },
    )
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

// ==================== BULK VALIDATION ====================

export interface BulkValidationResult {
  studentCode: string
  exists: boolean
  userId?: string
}

/**
 * Validate multiple student codes at once (optimized - single API call)
 * Used for Import Excel validation to replace N individual API calls
 */
export async function validateStudentsBulk(
  studentCodes: string[],
): Promise<BulkValidationResult[]> {
  try {
    const response = await API.post<ApiResponse<BulkValidationResult[]>>(
      'api/v1/students/validate-bulk',
      { studentCodes },
    )
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

// ==================== NEW: SMART IMPORT FEATURES ====================

export interface GetAvailableStudentsParams {
  pageNumber?: number
  pageSize?: number
  search?: string
  year?: number
  major?: string
  status?: string
  excludeClassId?: string
}

export interface StudentValidationResult {
  identifier: string
  isValid: boolean
  studentId?: string
  studentCode?: string
  fullName?: string
  email?: string
  errorMessage?: string
  isDuplicate?: boolean
}

export interface ValidateBatchRequest {
  identifiers: string[]
  classId?: string
}

/**
 * Get available students (not in a specific class) with filters
 */
export async function getAvailableStudents(
  params: GetAvailableStudentsParams,
): Promise<PagedResponse<StudentResponse>> {
  try {
    const queryString = buildQueryString(params)
    const response = await API.get<ApiResponse<BackendPagedResponse<StudentResponse>>>(
      `api/v1/students${queryString}`,
    )
    
    // Backend wraps BackendPagedResponse inside ApiResponse
    const result = unwrapApiResponse(response.data)
    
    return {
      data: result.items || [],
      page: result.pageNumber,
      pageSize: result.pageSize,
      totalCount: result.totalCount,
      totalPages: result.totalPages,
      hasPrevious: result.hasPreviousPage || false,
      hasNext: result.hasNextPage || false,
    }
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Download Excel template for importing students
 */
export async function downloadImportTemplate(classId?: string): Promise<Blob> {
  try {
    const params = classId ? { classId } : {}
    const queryString = buildQueryString(params)
    const response = await API.get(`api/v1/students/template${queryString}`, {
      responseType: 'blob',
    })
    return response.data
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Validate batch of students before import
 */
export async function validateBatch(
  request: ValidateBatchRequest,
): Promise<{
  totalCount: number
  validCount: number
  invalidCount: number
  results: StudentValidationResult[]
}> {
  try {
    const response = await API.post<
      ApiResponse<{
        totalCount: number
        validCount: number
        invalidCount: number
        results: StudentValidationResult[]
      }>
    >('api/v1/students/validate-batch', request)
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}
