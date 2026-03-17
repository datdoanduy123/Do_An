import api from './api';
import type { 
  NguoiDungDto, 
  TaoNguoiDungDto, 
  CapNhatNguoiDungDto, 
  NguoiDungQuery 
} from './UserTypes';
import type { PaginatedResult } from './PermissionTypes';

/**
 * Service xử lý các thao tác liên quan đến Người dùng.
 */
class UserService {
  /**
   * Lấy danh sách người dùng.
   */
  async getUsers(query: NguoiDungQuery = {}): Promise<PaginatedResult<NguoiDungDto>> {
    const response = await api.get('/NguoiDung/danh-sach', { params: query });
    return response.data.data;
  }

  /**
   * Lấy chi tiết người dùng.
   */
  async getUserById(id: number): Promise<NguoiDungDto> {
    const response = await api.get(`/NguoiDung/${id}`);
    return response.data.data;
  }

  /**
   * Lấy thông tin cá nhân người dùng đang đăng nhập.
   */
  async getProfile(): Promise<NguoiDungDto> {
    const response = await api.get('/NguoiDung/profile');
    return response.data.data;
  }

  /**
   * Tạo người dùng mới.
   */
  async createUser(data: TaoNguoiDungDto): Promise<any> {
    const response = await api.post('/NguoiDung/tao-nguoi-dung', data);
    return response.data;
  }

  /**
   * Cập nhật người dùng.
   */
  async updateUser(id: number, data: CapNhatNguoiDungDto): Promise<any> {
    const response = await api.put(`/NguoiDung/${id}`, data);
    return response.data;
  }

  /**
   * Xóa người dùng (xóa mềm).
   */
  async deleteUser(id: number): Promise<boolean> {
    const response = await api.delete(`/NguoiDung/${id}`);
    return response.data.statusCode === 200;
  }

  /**
   * Gán vai trò cho người dùng.
   */
  async assignRole(userId: number, vaiTroId: number): Promise<any> {
    const response = await api.post('/VaiTro/gan-vaitro', {
      nguoiDungId: userId,
      vaiTroId: vaiTroId
    });
    return response.data;
  }

  /**
   * Gỡ vai trò khỏi người dùng.
   */
  async removeRole(userId: number, vaiTroId: number): Promise<any> {
    const response = await api.post('/VaiTro/go-vaitro', {
      nguoiDungId: userId,
      vaiTroId: vaiTroId
    });
    return response.data;
  }

  /**
   * Gán kỹ năng cho người dùng.
   */
  async assignSkill(data: { nguoiDungId: number, kyNangId: number, level: number, soNamKinhNghiem: number }): Promise<any> {
    const response = await api.post('/NguoiDung/gan-kynang', data);
    return response.data;
  }

  /**
   * Gỡ kỹ năng khỏi người dùng.
   */
  async removeSkill(data: { nguoiDungId: number, kyNangId: number }): Promise<any> {
    const response = await api.post('/NguoiDung/go-kynang', data);
    return response.data;
  }
}

export default new UserService();
export * from './UserTypes';
