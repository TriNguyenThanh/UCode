import { API } from '../api'
import type {
  ApiResponse,
  Assignment,
  AssignmentProblemDetail,
  AssignmentStatistics,
  AssignmentUser,
  BestSubmission,
} from '../types'
import { handleApiError, unwrapApiResponse } from './utils'

// ==================== REQUEST TYPES ====================

export interface CreateAssignmentRequest {
  assignmentType: 'HOMEWORK' | 'EXAM' | 'PRACTICE' | 'CONTEST'
  classId: string
  title: string
  description?: string
  startTime?: string
  endTime?: string
  allowLateSubmission?: boolean
  status?: 'DRAFT' | 'SCHEDULED' | 'ACTIVE' | 'ENDED' | 'GRADED'
  problems: {
    problemId: string
    points: number
    orderIndex: number
  }[]
}

export interface UpdateAssignmentRequest extends CreateAssignmentRequest {}

export interface UpdateAssignmentStatusRequest {
  status: 'NOT_STARTED' | 'IN_PROGRESS' | 'SUBMITTED' | 'GRADED'
}

export interface GradeSubmissionRequest {
  score?: number
  teacherFeedback?: string
}

// ==================== ASSIGNMENT SERVICE ====================

/**
 * Creates a new assignment (Teacher only)
 */
export async function createAssignment(data: CreateAssignmentRequest): Promise<Assignment> {
  try {
    const response = await API.post<ApiResponse<Assignment>>('/api/v1/assignments/create', data)
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Updates an existing assignment (Teacher only)
 */
export async function updateAssignment(
  assignmentId: string,
  data: UpdateAssignmentRequest,
): Promise<Assignment> {
  try {
    const response = await API.put<ApiResponse<Assignment>>(
      `/api/v1/assignments/update/${assignmentId}`,
      data,
    )
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Deletes an assignment (Teacher only)
 */
export async function deleteAssignment(assignmentId: string): Promise<void> {
  try {
    const response = await API.delete<ApiResponse<object>>(
      `/api/v1/assignments/delete/${assignmentId}`,
    )
    unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Gets an assignment by ID with basic problem info (Teacher only)
 */
export async function getAssignment(assignmentId: string): Promise<Assignment> {
  try {
    const response = await API.get<ApiResponse<Assignment>>(`/api/v1/assignments/${assignmentId}`)
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Gets all assignments created by the current teacher
 */
export async function getMyAssignments(): Promise<Assignment[]> {
  try {
    const response = await API.get<ApiResponse<Assignment[]>>('/api/v1/assignments/my-assignments')
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Gets all assignments for a specific class
 */
export async function getAssignmentsByClass(classId: string): Promise<Assignment[]> {
  try {
    const response = await API.get<ApiResponse<Assignment[]>>(
      `/api/v1/assignments/class/${classId}`,
    )
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Gets all students and their status for a specific assignment (Teacher only)
 */
export async function getAssignmentStudents(assignmentId: string): Promise<AssignmentUser[]> {
  try {
    const response = await API.get<ApiResponse<AssignmentUser[]>>(
      `/api/v1/assignments/${assignmentId}/students`,
    )
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Gets statistics for a specific assignment (Teacher only)
 */
export async function getAssignmentStatistics(
  assignmentId: string,
): Promise<AssignmentStatistics> {
  try {
    const response = await API.get<ApiResponse<AssignmentStatistics>>(
      `/api/v1/assignments/${assignmentId}/statistics`,
    )
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

// ==================== STUDENT ENDPOINTS ====================

/**
 * Gets all assignments assigned to the current student
 */
export async function getStudentAssignments(): Promise<Assignment[]> {
  try {
    const response = await API.get<ApiResponse<Assignment[]>>(
      '/api/v1/assignments/student/my-assignments',
    )
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Gets assignment detail for the current student
 */
export async function getMyAssignmentDetail(assignmentId: string): Promise<AssignmentUser> {
  try {
    const response = await API.get<ApiResponse<AssignmentUser>>(
      `/api/v1/assignments/${assignmentId}/student/my-detail`,
    )
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Updates assignment status for the current student
 */
export async function updateMyAssignmentStatus(
  assignmentId: string,
  data: UpdateAssignmentStatusRequest,
): Promise<AssignmentUser> {
  try {
    const response = await API.put<ApiResponse<AssignmentUser>>(
      `/api/v1/assignments/${assignmentId}/student/update-status`,
      data,
    )
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Starts an assignment for the current student
 */
export async function startAssignment(assignmentId: string): Promise<AssignmentUser> {
  try {
    const response = await API.post<ApiResponse<AssignmentUser>>(
      `/api/v1/assignments/${assignmentId}/student/start`,
    )
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

// ==================== GRADING ENDPOINTS ====================

/**
 * Grades a specific submission (Teacher only)
 */
export async function gradeSubmission(
  assignmentId: string,
  submissionId: string,
  data: GradeSubmissionRequest,
): Promise<BestSubmission> {
  try {
    const response = await API.put<ApiResponse<BestSubmission>>(
      `/api/v1/assignments/${assignmentId}/grade-submission/${submissionId}`,
      data,
    )
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}
