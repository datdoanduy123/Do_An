import api from './api';

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
      const response = await api.post('/TaiKhoan/dang-nhap', data);
      // Backend trả về { StatusCode, Message, Data }
      const resData = response.data;
      
      return {
        success: resData.statusCode === 200,
        message: resData.message,
        data: resData.data
      };
    } catch (error: any) {
      if (error.response && error.response.data) {
        const resData = error.response.data;
        return {
          success: false,
          message: resData.message || 'Lỗi đăng nhập.',
          data: resData.details
        };
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
