import { API } from '../api'
import type {
  ApiResponse,
  PagedResponse,
  User,
  UpdateUserByAdminRequest,
  UserStatus,
  UserRole,
} from '../types'
import { handleApiError, unwrapApiResponse, buildQueryString } from './utils'

// ==================== REQUEST TYPES ====================

export interface GetUsersParams {
  pageNumber?: number
  pageSize?: number
  role?: UserRole
  status?: UserStatus
  search?: string
}

// ==================== USER SERVICE ====================

/**
 * Get all users with pagination and filters (Admin only)
 */
export async function getAllUsers(params?: GetUsersParams): Promise<PagedResponse<User>> {
  try {
    const queryString = buildQueryString(params || {})
    const response = await API.get<ApiResponse<PagedResponse<User>>>(
      `api/v1/users${queryString}`,
    )
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Get user by ID (Admin only)
 */
export async function getUserById(id: string): Promise<User> {
  try {
    const response = await API.get<ApiResponse<User>>(`api/v1/users/${id}`)
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Get user by email (Admin only)
 */
export async function getUserByEmail(email: string): Promise<User> {
  try {
    const response = await API.get<ApiResponse<User>>(`api/v1/users/by-email/${email}`)
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Get user by username (Admin only)
 */
export async function getUserByUsername(username: string): Promise<User> {
  try {
    const response = await API.get<ApiResponse<User>>(`api/v1/users/by-username/${username}`)
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Update user by ID (Admin only)
 */
export async function updateUser(id: string, data: UpdateUserByAdminRequest): Promise<User> {
  try {
    const response = await API.put<ApiResponse<User>>(`api/v1/users/${id}`, data)
    return unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Update user status (Admin only)
 */
export async function updateUserStatus(userId: string, status: UserStatus): Promise<void> {
  try {
    const response = await API.patch<ApiResponse<object>>('api/v1/users/update-status', {
      userId,
      status,
    })
    unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}

/**
 * Delete user by ID (Admin only)
 */
export async function deleteUser(id: string): Promise<void> {
  try {
    const response = await API.delete<ApiResponse<object>>(`api/v1/users/delete?id=${id}`)
    unwrapApiResponse(response.data)
  } catch (error) {
    handleApiError(error)
  }
}
