import { API } from '../api'
import type { ApiResponse, Dataset, DatasetKind, TestCase } from '../types'
import { handleApiError, unwrapApiResponse, buildQueryString } from './utils'

// ==================== REQUEST TYPES ====================

export interface CreateDatasetRequest {
  problemId: string
  name: string
  kind: DatasetKind
  testCases: {
    input: string
    expectedOutput: string
    orderIndex: number
  }[]
}

export interface UpdateDatasetRequest {
  datasetId: string
  problemId?: string
  name?: string
  kind?: DatasetKind
  testCases?: {
    testCaseId?: string
    datasetId?: string
    input: string
    expectedOutput: string
    orderIndex: number
  }[]
}

// ==================== DATASET SERVICE ====================

/**
 * Creates a new dataset (test case) (Teacher only)
 */
export async function createDataset(data: CreateDatasetRequest): Promise<Dataset> {
  try {
    const response = await API.post<ApiResponse<Dataset>>('/api/v1/datasets/create', data)
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Updates an existing dataset (test case) (Teacher only)
 */
export async function updateDataset(data: UpdateDatasetRequest): Promise<Dataset> {
  try {
    const response = await API.put<ApiResponse<Dataset>>('/api/v1/datasets/update', data)
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Deletes a dataset by ID (Teacher only)
 */
export async function deleteDataset(datasetId: string): Promise<void> {
  try {
    const response = await API.delete<ApiResponse<Dataset>>(
      `/api/v1/datasets/del${buildQueryString({ uid: datasetId })}`,
    )
    unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Retrieves a specific dataset by ID with details (Teacher only)
 */
export async function getDataset(datasetId: string): Promise<Dataset> {
  try {
    const response = await API.get<ApiResponse<Dataset>>(
      `/api/v1/datasets/get${buildQueryString({ uid: datasetId })}`,
    )
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Retrieves datasets (test cases) for a specific problem (Teacher only)
 * This endpoint is available via ProblemController
 */
export async function getDatasetsByProblem(problemId: string): Promise<Dataset[]> {
  try {
    const response = await API.get<ApiResponse<Dataset[]>>(
      `/api/v1/problems/get-datasets${buildQueryString({ problemId })}`,
    )
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}
