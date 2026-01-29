import axios from "axios";

// Lấy token từ localStorage
const getToken = () => localStorage.getItem("token");

// Base URL cho API Gateway - tất cả requests đi qua API Gateway
const API_BASE_URL = import.meta.env.VITE_APP_SERVER_URL || 'http://localhost:5000';

export const API = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  // withCredentials: true, // Tắt vì sử dụng JWT qua Authorization header
});

// Thêm token vào mọi request
API.interceptors.request.use((config) => {
  const token = getToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Xử lý response errors, đặc biệt là 401 (Unauthorized)
API.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;
    
    // Nếu nhận được 401 và chưa retry
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      
      try {
        // Thử refresh token
        const refreshToken = localStorage.getItem('refreshToken');
        if (refreshToken) {
          const response = await axios.post(`${API_BASE_URL}/api/v1/auth/refresh-token`, {
            refreshToken
          });
          
          if (response.data.success && response.data.data) {
            const { accessToken, refreshToken: newRefreshToken } = response.data.data;
            
            // Cập nhật tokens
            localStorage.setItem('token', accessToken);
            localStorage.setItem('refreshToken', newRefreshToken);
            
            // Retry request với token mới
            originalRequest.headers.Authorization = `Bearer ${accessToken}`;
            return API(originalRequest);
          }
        }
      } catch (refreshError) {
        // Refresh token thất bại
        console.error('Token refresh failed:', refreshError);
      }
      
      // Nếu refresh thất bại hoặc không có refresh token, clear storage và redirect
      localStorage.removeItem('token');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('email');
      localStorage.removeItem('role');
      localStorage.removeItem('userId');
      localStorage.removeItem('userName');
      localStorage.removeItem('username');
      localStorage.removeItem('isFaceAuth');
      
      // Redirect to login
      window.location.href = '/login';
    }
    
    return Promise.reject(error);
  }
);
