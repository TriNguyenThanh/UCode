import { API } from '../api'
import type { ApiResponse, Language } from '../types'
import { handleApiError, unwrapApiResponse, buildQueryString } from './utils'

// Re-export Language type for convenience
export type { Language } from '../types'

// ==================== REQUEST TYPES ====================

export interface CreateLanguageRequest {
  code: string
  displayName: string
  defaultTimeFactor: number
  defaultMemoryKb: number
  defaultHead: string
  defaultBody: string
  defaultTail: string
  isEnabled?: boolean
  displayOrder?: number
}

export interface UpdateLanguageRequest {
  code?: string
  displayName?: string
  defaultTimeFactor?: number
  defaultMemoryKb?: number
  defaultHead?: string
  defaultBody?: string
  defaultTail?: string
  isEnabled?: boolean
  displayOrder?: number
}

// ==================== LANGUAGE SERVICE ====================

/**
 * Get all available programming languages
 */
export async function getAllLanguages(includeDisabled = false): Promise<Language[]> {
  try {
    const response = await API.get<ApiResponse<Language[]>>(
      `/api/v1/languages${buildQueryString({ includeDisabled })}`,
    )
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Get a specific language by ID
 */
export async function getLanguage(languageId: string): Promise<Language> {
  try {
    const response = await API.get<ApiResponse<Language>>(`/api/v1/languages/${languageId}`)
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Get a specific language by code
 */
export async function getLanguageByCode(code: string): Promise<Language> {
  try {
    const response = await API.get<ApiResponse<Language>>(`/api/v1/languages/by-code/${code}`)
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Create a new language (Admin only)
 */
export async function createLanguage(data: CreateLanguageRequest): Promise<Language> {
  try {
    const response = await API.post<ApiResponse<Language>>('/api/v1/languages', data)
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Update an existing language (Admin only)
 */
export async function updateLanguage(
  languageId: string,
  data: UpdateLanguageRequest,
): Promise<Language> {
  try {
    const response = await API.put<ApiResponse<Language>>(`/api/v1/languages/${languageId}`, data)
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Delete a language (Admin only) - Soft delete by disabling
 */
export async function deleteLanguage(languageId: string): Promise<void> {
  try {
    const response = await API.delete<ApiResponse<boolean>>(`/api/v1/languages/${languageId}`)
    unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Enable a disabled language (Admin only)
 */
export async function enableLanguage(languageId: string): Promise<void> {
  try {
    const response = await API.post<ApiResponse<boolean>>(`/api/v1/languages/${languageId}/enable`)
    unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}
