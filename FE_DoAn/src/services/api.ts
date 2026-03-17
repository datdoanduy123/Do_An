import axios from 'axios';

const API_URL = 'http://localhost:5095/api';

/**
 * Tạo một instance axios với cấu hình cơ bản.
 */
const api = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

/**
 * Interceptor để tự động thêm Token vào Header Authorization của mỗi yêu cầu.
 */
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('accessToken');
    if (token) {
      console.log('Sending request with Token:', token.substring(0, 10) + '...');
      config.headers.Authorization = `Bearer ${token}`;
    } else {
      console.warn('No token found in localStorage');
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

/**
 * Interceptor để xử lý các lỗi chung (như 401).
 */
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response && error.response.status === 401) {
      // Có thể xử lý logout hoặc chuyển hướng đăng nhập ở đây nếu cần
      console.warn('Unauthorized! Redirecting to login...');
      // localStorage.removeItem('accessToken');
      // window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default api;
