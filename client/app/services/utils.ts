import type { ApiResponse, ErrorResponse } from '../types'

/**
 * Handle API response errors
 * Throws an error with a descriptive message based on the response
 */
export function handleApiError(error: any): never {
  // If error has response from server
  if (error.response) {
    const data = error.response.data as ApiResponse<any> | ErrorResponse
    
    // Check if it's an ApiResponse with success=false
    if ('success' in data && !data.success) {
      const message = data.message || 'An error occurred'
      const errors = data.errors?.join(', ') || ''
      throw new Error(errors ? `${message}: ${errors}` : message)
    }
    
    // Check if it's an ErrorResponse
    if ('error' in data) {
      const errorData = data as ErrorResponse
      if (errorData.errors) {
        // Validation errors: { field: [messages] }
        const validationMessages = Object.entries(errorData.errors)
          .map(([field, messages]) => `${field}: ${messages.join(', ')}`)
          .join('; ')
        throw new Error(`${errorData.message || 'Validation failed'}: ${validationMessages}`)
      }
      throw new Error(errorData.message || errorData.error)
    }
    
    // Generic error response
    throw new Error(error.response.data?.message || error.response.statusText || 'Server error')
  }
  
  // Network error or no response
  if (error.request) {
    throw new Error('No response from server. Please check your connection.')
  }
  
  // Other errors
  throw new Error(error.message || 'An unexpected error occurred')
}

/**
 * Unwrap ApiResponse data or throw error if unsuccessful
 */
export function unwrapApiResponse<T>(response: ApiResponse<T>): T {
  if (!response.success || response.data === undefined) {
    const message = response.message || 'Request failed'
    const errors = response.errors?.join(', ') || ''
    throw new Error(errors ? `${message}: ${errors}` : message)
  }
  return response.data
}

/**
 * Build query string from params object
 */
export function buildQueryString(params: Record<string, any>): string {
  const filteredParams = Object.entries(params)
    .filter(([_, value]) => value !== undefined && value !== null && value !== '')
    .map(([key, value]) => `${encodeURIComponent(key)}=${encodeURIComponent(String(value))}`)
    .join('&')
  
  return filteredParams ? `?${filteredParams}` : ''
}
