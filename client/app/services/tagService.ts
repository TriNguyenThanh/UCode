import { API } from '../api'
import type { ApiResponse, Tag, Problem, TagCategory } from '../types'
import { handleApiError, unwrapApiResponse, buildQueryString } from './utils'

// ==================== REQUEST TYPES ====================

export interface CreateTagRequest {
  name: string
  category: TagCategory
  color?: string
}

export interface UpdateTagRequest {
  name?: string
  category?: TagCategory
  color?: string
}

// ==================== TAG SERVICE ====================

/**
 * Get all tags with optional category filter
 */
export async function getAllTags(category?: TagCategory): Promise<Tag[]> {
  try {
    const response = await API.get<ApiResponse<Tag[]>>(
      `/api/v1/tags${buildQueryString({ category })}`,
    )
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Get a specific tag by ID
 */
export async function getTag(tagId: string): Promise<Tag> {
  try {
    const response = await API.get<ApiResponse<Tag>>(`/api/v1/tags/${tagId}`)
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Create a new tag (Teacher only)
 */
export async function createTag(data: CreateTagRequest): Promise<Tag> {
  try {
    const response = await API.post<ApiResponse<Tag>>('/api/v1/tags/create', data)
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Update an existing tag (Teacher only)
 */
export async function updateTag(tagId: string, data: UpdateTagRequest): Promise<Tag> {
  try {
    const response = await API.put<ApiResponse<Tag>>(`/api/v1/tags/update/${tagId}`, data)
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Delete a tag (Admin only)
 */
export async function deleteTag(tagId: string): Promise<void> {
  try {
    const response = await API.delete<ApiResponse<boolean>>(`/api/v1/tags/${tagId}`)
    unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Get all problems that use a specific tag
 */
export async function getProblemsByTag(tagId: string): Promise<Problem[]> {
  try {
    const response = await API.get<ApiResponse<Problem[]>>(`/api/v1/tags/${tagId}/problems`)
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}
