import { API } from './api'

export type User = { 
  email: string
  token: string
  role: 'student' | 'teacher' | 'admin'
  name?: string
  userId?: string
} | null

interface ApiResponse<T> {
  success: boolean
  data: T | null
  message: string | null
  errors: string[] | null
}

interface LoginData {
  accessToken: string
  refreshToken: string
  expiresAt: number
  user: {
    userId: string
    username: string
    email: string
    fullName: string
    role: 'Student' | 'Teacher' | 'Admin' | number
    status: number
    createdAt: string
    updatedAt: string | null
    lastLoginAt: string | null
  }
}

type LoginResponse = ApiResponse<LoginData>

export const auth = {
  /**
   * Login user
   */
  async login(email: string, password: string): Promise<NonNullable<User>> {
    try {
      // Gọi API backend
      const response = await API.post<LoginResponse>('api/v1/auth/login', {
        emailOrUsername: email,
        password,
        rememberMe: false
      })

      if (!response.data.success || !response.data.data) {
        throw new Error(response.data.message || 'Đăng nhập thất bại')
      }

      const { accessToken, refreshToken, user } = response.data.data
      
      // Chuyển đổi role từ backend (Student/Teacher/Admin) sang lowercase
      const roleStr = String(user.role).toLowerCase()
      const role = (roleStr === 'admin' || roleStr === 'teacher' || roleStr === 'student') 
        ? roleStr as 'student' | 'teacher' | 'admin'
        : 'student' // default fallback
      
      // Lưu vào localStorage
      localStorage.setItem('token', accessToken)
      localStorage.setItem('refreshToken', refreshToken)
      localStorage.setItem('email', user.email)
      localStorage.setItem('role', role)
      localStorage.setItem('userId', user.userId)
      localStorage.setItem('userName', user.fullName)
      localStorage.setItem('username', user.username)
      
      return { 
        token: accessToken, 
        email: user.email, 
        role,
        name: user.fullName,
        userId: user.userId
      }
    } catch (error: any) {
      // Xử lý lỗi từ API
      const message = error.response?.data?.message || error.message || 'Đăng nhập thất bại. Vui lòng kiểm tra lại thông tin đăng nhập.'
      const err = new Error(message)
      // @ts-expect-error attach status for convenience
      err.status = error.response?.status || 401
      throw err
    }
  },

  /**
   * Refresh access token
   */
  async refreshToken(): Promise<string> {
    try {
      const refreshToken = localStorage.getItem('refreshToken')
      if (!refreshToken) {
        throw new Error('No refresh token available')
      }

      const response = await API.post<LoginResponse>('api/v1/auth/refresh-token', {
        refreshToken
      })

      if (!response.data.success || !response.data.data) {
        throw new Error(response.data.message || 'Token refresh failed')
      }

      const { accessToken, refreshToken: newRefreshToken } = response.data.data
      
      // Update tokens
      localStorage.setItem('token', accessToken)
      localStorage.setItem('refreshToken', newRefreshToken)
      
      return accessToken
    } catch (error: any) {
      // Clear tokens and redirect to login
      this.logout()
      window.location.href = '/login'
      throw error
    }
  },

  /**
   * Logout user
   */
  async logout(): Promise<void> {
    try {
      const refreshToken = localStorage.getItem('refreshToken')
      if (refreshToken) {
        // Call logout API
        await API.post('api/v1/auth/logout', { refreshToken })
      }
    } catch (error) {
      // Ignore logout errors
      console.error('Logout error:', error)
    } finally {
      // Always clear localStorage
      localStorage.removeItem('token')
      localStorage.removeItem('refreshToken')
      localStorage.removeItem('email')
      localStorage.removeItem('role')
      localStorage.removeItem('userId')
      localStorage.removeItem('userName')
      localStorage.removeItem('username')
    }
  },

  /**
   * Change password (self-service)
   */
  async changePassword(currentPassword: string, newPassword: string): Promise<void> {
    try {
      const response = await API.post<ApiResponse<void>>('api/v1/auth/change-password', {
        currentPassword,
        newPassword
      })

      if (!response.data.success) {
        throw new Error(response.data.message || 'Đổi mật khẩu thất bại')
      }
    } catch (error: any) {
      const message = error.response?.data?.message || error.message || 'Đổi mật khẩu thất bại'
      throw new Error(message)
    }
  },

  /**
   * Forgot password - Request reset
   */
  async forgotPassword(email: string): Promise<void> {
    try {
      const response = await API.post<ApiResponse<void>>('api/v1/auth/forgot-password', {
        email
      })

      if (!response.data.success) {
        throw new Error(response.data.message || 'Yêu cầu đặt lại mật khẩu thất bại')
      }
    } catch (error: any) {
      const message = error.response?.data?.message || error.message || 'Yêu cầu đặt lại mật khẩu thất bại'
      throw new Error(message)
    }
  },

  /**
   * Reset password with token
   */
  async resetPassword(email: string, token: string, newPassword: string): Promise<void> {
    try {
      const response = await API.post<ApiResponse<void>>('api/v1/auth/reset-password', {
        email,
        token,
        newPassword
      })

      if (!response.data.success) {
        throw new Error(response.data.message || 'Đặt lại mật khẩu thất bại')
      }
    } catch (error: any) {
      const message = error.response?.data?.message || error.message || 'Đặt lại mật khẩu thất bại'
      throw new Error(message)
    }
  },
  
  /**
   * Get current user from localStorage
   */
  getUser(): User {
    const token = localStorage.getItem('token')
    const email = localStorage.getItem('email')
    const role = localStorage.getItem('role') as 'student' | 'teacher' | 'admin' | null
    const name = localStorage.getItem('userName')
    const userId = localStorage.getItem('userId')
    
    if (!token) return null
    
    return { 
      email: email ?? '', 
      token, 
      role: role ?? 'student',
      name: name ?? undefined,
      userId: userId ?? undefined
    }
  },

  /**
   * Check if user is authenticated
   */
  isAuthenticated(): boolean {
    return !!localStorage.getItem('token')
  },

  /**
   * Get access token
   */
  getToken(): string | null {
    return localStorage.getItem('token')
  },

  /**
   * Get refresh token
   */
  getRefreshToken(): string | null {
    return localStorage.getItem('refreshToken')
  }
}
