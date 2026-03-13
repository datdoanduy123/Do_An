import axios from 'axios';

const API_URL = 'http://localhost:5095/api/TaiKhoan'; // Cần điều chỉnh URL phù hợp với môi trường chạy Backend

export interface DangNhapDto {
  tenDangNhap: string;
  matKhau: string;
}

export interface LoginResponse {
  data: any;
  message: string;
  success: boolean;
}

/**
 * Service xử lý các nghiệp vụ liên quan đến xác thực người dùng.
 */
class AuthService {
  /**
   * Gửi yêu cầu đăng nhập đến Backend.
   * @param data Thông tin đăng nhập (tên đăng nhập và mật khẩu).
   * @returns Kết quả từ API.
   */
  async login(data: DangNhapDto): Promise<LoginResponse> {
    try {
      const response = await axios.post(`${API_URL}/dang-nhap`, data);
      return response.data;
    } catch (error: any) {
      if (error.response) {
        return error.response.data;
      }
      return {
        success: false,
        message: 'Không thể kết nối đến máy chủ.',
        data: null
      };
    }
  }

  /**
   * Lưu token hoặc thông tin người dùng vào local storage.
   */
  setSession(token: string) {
    localStorage.setItem('accessToken', token);
  }

  /**
   * Đăng xuất người dùng.
   */
  logout() {
    localStorage.removeItem('accessToken');
  }
}

export default new AuthService();
