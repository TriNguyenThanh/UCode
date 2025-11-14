import axios from "axios";

// Lấy token từ localStorage
const getToken = () => localStorage.getItem("token");

// Base URL cho API Gateway - tất cả requests đi qua API Gateway
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';

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
