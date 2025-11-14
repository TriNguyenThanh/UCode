import { API } from '../api'
import type {
  ApiResponse,
  Problem,
  ProblemLanguage,
  ProblemAsset,
  PagedResponse,
  Difficulty,
  Visibility,
  ProblemStatus,
  IoMode,
  AssetType,
  ContentFormat,
} from '../types'
import { handleApiError, unwrapApiResponse, buildQueryString } from './utils'

// ==================== REQUEST TYPES ====================

export interface CreateProblemRequest {
  code?: string
  title: string
  difficulty: Difficulty
  visibility: Visibility
}

export interface UpdateProblemRequest {
  problemId: string
  code?: string
  slug?: string
  title?: string
  difficulty?: Difficulty
  visibility?: Visibility
  status?: ProblemStatus
  timeLimitMs?: number
  memoryLimitKb?: number
  sourceLimitKb?: number
  stackLimitKb?: number
  ioMode?: IoMode
  statement?: string
  solution?: string
  inputFormat?: string
  outputFormat?: string
  constraints?: string
  validatorRef?: string
  changelog?: string
  isLocked?: boolean
  problemAssets?: CreateProblemAssetRequest[]
}

export interface CreateProblemAssetRequest {
  type: AssetType
  objectRef: string
  checksum?: string
  title?: string
  format: ContentFormat
  orderIndex: number
  isActive?: boolean
  createdBy?: string
}

export interface UpdateProblemAssetRequest {
  type?: AssetType
  objectRef?: string
  checksum?: string
  title?: string
  format?: ContentFormat
  orderIndex?: number
  isActive?: boolean
}

export interface ProblemLanguageRequest {
  problemId: string
  languageId: string
  timeFactor?: number
  memoryKb?: number
  head?: string
  body?: string
  tail?: string
  isAllowed: boolean
}

export interface SearchProblemsParams {
  keyword?: string
  difficulty?: string
  page?: number
  pageSize?: number
}

// ==================== PROBLEM SERVICE ====================

/**
 * Creates a new problem (Teacher only)
 */
export async function createProblem(data: CreateProblemRequest): Promise<Problem> {
  try {
    const response = await API.post<ApiResponse<Problem>>('/api/v1/problems/create', data)
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Gets a specific problem by ID (Teacher only)
 */
export async function getProblem(problemId: string): Promise<Problem> {
  try {
    const response = await API.get<ApiResponse<Problem>>(`/api/v1/problems/${problemId}`)
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Gets a specific problem by ID (Student) - Only public/published problems
 */
export async function getProblemForStudent(problemId: string): Promise<Problem> {
  try {
    const response = await API.get<ApiResponse<Problem>>(
      `/api/v1/problems/student/get/${problemId}`,
    )
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Gets all problems created by the current teacher with pagination
 */
export async function getMyProblems(page = 1, pageSize = 20): Promise<PagedResponse<Problem>> {
  try {
    const response = await API.get<ApiResponse<PagedResponse<Problem>>>(
      `/api/v1/problems/all-problems${buildQueryString({ page, pageSize })}`,
    )
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Deletes a problem by ID (Teacher only)
 */
export async function deleteProblem(problemId: string): Promise<void> {
  try {
    const response = await API.delete<ApiResponse<boolean>>(
      `/api/v1/problems/del${buildQueryString({ uid: problemId })}`,
    )
    unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Updates an existing problem (Teacher only)
 */
export async function updateProblem(data: UpdateProblemRequest): Promise<Problem> {
  try {
    const response = await API.put<ApiResponse<Problem>>('/api/v1/problems/update', data)
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Gets public problems available for students
 */
export async function getPublicProblems(): Promise<Problem[]> {
  try {
    const response = await API.get<ApiResponse<Problem[]>>(
      '/api/v1/problems/student/get-public-problems',
    )
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Search problems with filters
 */
export async function searchProblems(params: SearchProblemsParams): Promise<PagedResponse<Problem>> {
  try {
    const response = await API.get<ApiResponse<PagedResponse<Problem>>>(
      `/api/v1/problems/search${buildQueryString(params)}`,
    )
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Search problems by tag name
 */
export async function getProblemsByTag(tagName: string): Promise<Problem[]> {
  try {
    const response = await API.get<ApiResponse<Problem[]>>(`/api/v1/problems/by-tag/${tagName}`)
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

// ==================== PROBLEM ASSET ENDPOINTS ====================

/**
 * Get all assets for a problem (Teacher only)
 */
export async function getProblemAssets(problemId: string): Promise<ProblemAsset[]> {
  try {
    const response = await API.get<ApiResponse<ProblemAsset[]>>(
      `/api/v1/problems/${problemId}/assets`,
    )
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Add a new asset to a problem (Teacher only)
 */
export async function addProblemAsset(
  problemId: string,
  data: CreateProblemAssetRequest,
): Promise<ProblemAsset> {
  try {
    const response = await API.post<ApiResponse<ProblemAsset>>(
      `/api/v1/problems/${problemId}/assets`,
      data,
    )
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Update an existing problem asset (Teacher only)
 */
export async function updateProblemAsset(
  problemId: string,
  assetId: string,
  data: UpdateProblemAssetRequest,
): Promise<ProblemAsset> {
  try {
    const response = await API.put<ApiResponse<ProblemAsset>>(
      `/api/v1/problems/${problemId}/assets/${assetId}`,
      data,
    )
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Delete a problem asset (Teacher only)
 */
export async function deleteProblemAsset(problemId: string, assetId: string): Promise<void> {
  try {
    const response = await API.delete<ApiResponse<boolean>>(
      `/api/v1/problems/${problemId}/assets/${assetId}`,
    )
    unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

// ==================== TAG ENDPOINTS ====================

/**
 * Add tags to a problem (Teacher only)
 */
export async function addTagsToProblem(problemId: string, tagIds: string[]): Promise<void> {
  try {
    const response = await API.post<ApiResponse<boolean>>(
      `/api/v1/problems/${problemId}/tags`,
      tagIds,
    )
    unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Remove a tag from a problem (Teacher only)
 */
export async function removeTagFromProblem(problemId: string, tagId: string): Promise<void> {
  try {
    const response = await API.delete<ApiResponse<boolean>>(
      `/api/v1/problems/${problemId}/tags/${tagId}`,
    )
    unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

// ==================== PROBLEM LANGUAGE ENDPOINTS ====================

/**
 * Get available languages for a problem (Teacher only)
 */
export async function getAvailableLanguagesForProblem(
  problemId: string,
): Promise<ProblemLanguage[]> {
  try {
    const response = await API.get<ApiResponse<ProblemLanguage[]>>(
      `/api/v1/problems/${problemId}/available-languages`,
    )
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Add or update language configurations for a problem (batch operation) (Teacher only)
 */
export async function addOrUpdateProblemLanguages(
  problemId: string,
  data: ProblemLanguageRequest[],
): Promise<ProblemLanguage[]> {
  try {
    const response = await API.post<ApiResponse<ProblemLanguage[]>>(
      `/api/v1/problems/${problemId}/languages`,
      data,
    )
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Delete a language override for a problem (Teacher only)
 */
export async function deleteProblemLanguage(problemId: string, languageId: string): Promise<void> {
  try {
    const response = await API.delete<ApiResponse<boolean>>(
      `/api/v1/problems/${problemId}/languages/${languageId}`,
    )
    unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}
