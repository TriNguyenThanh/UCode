import { API } from '../api'
import type { ApiResponse, Submission, BestSubmission, SubmissionStatus } from '../types'
import { handleApiError, unwrapApiResponse, buildQueryString } from './utils'

// ==================== REQUEST TYPES ====================

export interface SubmitCodeRequest {
  problemId: string
  languageId: string
  sourceCode: string
  assignmentId: string | null
}

export interface CreateSubmissionResponse {
  submissionId: string
  problemId: string
  userId: string
  languageCode: string
  status: SubmissionStatus
  submittedAt: string
}

// ==================== SUBMISSION SERVICE ====================

/**
 * Get a specific submission by ID
 */
export async function getSubmission(submissionId: string): Promise<Submission> {
  try {
    const response = await API.get<ApiResponse<Submission>>(
      `/api/v1/submissions/${submissionId}`,
    )
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Submit code for a problem
 */
export async function submitCode(data: SubmitCodeRequest): Promise<CreateSubmissionResponse> {
  try {
    const response = await API.post<ApiResponse<CreateSubmissionResponse>>(
      '/api/v1/submissions/submit-code',
      data,
    )
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Run code for a problem (test without submitting)
 */
export async function runCode(data: SubmitCodeRequest): Promise<CreateSubmissionResponse> {
  try {
    const response = await API.post<ApiResponse<CreateSubmissionResponse>>(
      '/api/v1/submissions/run-code',
      data,
    )
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Get all submissions for the authenticated user with pagination
 */
export async function getUserSubmissions(
  pageNumber = 1,
  pageSize = 10,
): Promise<Submission[]> {
  try {
    const response = await API.get<ApiResponse<Submission[]>>(
      `/api/v1/submissions/user${buildQueryString({ pageNumber, pageSize })}`,
    )
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Get all submissions for a specific problem by the authenticated user
 */
export async function getSubmissionsByProblem(
  problemId: string,
  pageNumber = 1,
  pageSize = 10,
): Promise<Submission[]> {
  try {
    const response = await API.get<ApiResponse<Submission[]>>(
      `/api/v1/submissions/problem/${problemId}${buildQueryString({ pageNumber, pageSize })}`,
    )
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Get best submissions (leaderboard) for a specific problem in an assignment
 */
export async function getBestSubmissions(
  assignmentId: string,
  problemId: string,
  pageNumber = 1,
  pageSize = 10,
): Promise<BestSubmission[]> {
  try {
    const response = await API.get<ApiResponse<BestSubmission[]>>(
      `/api/v1/submissions/assignment/${assignmentId}/problem/${problemId}/best${buildQueryString({ pageNumber, pageSize })}`,
    )
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Get best submissions (leaderboard) for a specific problem in an assignment
 */
export async function getListBestSubmissions(
  assignmentId: string,
  problems: string[]
): Promise<BestSubmission[]> {
  try {
    const response = await API.post<ApiResponse<BestSubmission[]>>(
      `/api/v1/submissions/assignment/${assignmentId}/problem/list-my-best`,
      { problemIds: problems }
    )
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Get the total number of submissions for the authenticated user
 */
export async function getUserSubmissionCount(): Promise<number> {
  try {
    const response = await API.get<ApiResponse<number>>('/api/v1/submissions/user/count')
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Get the number of submissions for a specific problem by the authenticated user
 */
export async function getSubmissionCountPerProblem(
  assignmentId: string,
  problemId: string,
): Promise<number> {
  try {
    const response = await API.get<ApiResponse<number>>(
      `/api/v1/submissions/assignment/${assignmentId}/problem/${problemId}/count`,
    )
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Get best submission for a specific student for a problem in an assignment (Teacher only)
 */
export async function getBestSubmissionForStudent(
  assignmentId: string,
  problemId: string,
  userId: string,
): Promise<BestSubmission> {
  try {
    const response = await API.get<ApiResponse<BestSubmission>>(
      `/api/v1/submissions/assignment/${assignmentId}/problem/${problemId}/student/${userId}/best`,
    )
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}
