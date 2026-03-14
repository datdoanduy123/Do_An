import axios from 'axios';
import type { 
  NguoiDungDto, 
  TaoNguoiDungDto, 
  CapNhatNguoiDungDto, 
  NguoiDungQuery 
} from './UserTypes';
import type { PaginatedResult } from './PermissionTypes';

const API_URL = 'http://localhost:5095/api';

/**
 * Service xử lý các thao tác liên quan đến Người dùng.
 */
class UserService {
  /**
   * Lấy danh sách người dùng.
   */
  async getUsers(query: NguoiDungQuery = {}): Promise<PaginatedResult<NguoiDungDto>> {
    const response = await axios.get(`${API_URL}/NguoiDung/danh-sach`, { params: query });
    return response.data.data;
  }

  /**
   * Lấy chi tiết người dùng.
   */
  async getUserById(id: number): Promise<NguoiDungDto> {
    const response = await axios.get(`${API_URL}/NguoiDung/${id}`);
    return response.data.data;
  }

  /**
   * Tạo người dùng mới.
   */
  async createUser(data: TaoNguoiDungDto): Promise<any> {
    const response = await axios.post(`${API_URL}/NguoiDung/tao-nguoi-dung`, data);
    return response.data;
  }

  /**
   * Cập nhật người dùng.
   */
  async updateUser(id: number, data: CapNhatNguoiDungDto): Promise<any> {
    const response = await axios.put(`${API_URL}/NguoiDung/${id}`, data);
    return response.data;
  }

  /**
   * Xóa người dùng (xóa mềm).
   */
  async deleteUser(id: number): Promise<boolean> {
    const response = await axios.delete(`${API_URL}/NguoiDung/${id}`);
    return response.data.statusCode === 200;
  }

  /**
   * Gán vai trò cho người dùng.
   */
  async assignRole(userId: number, vaiTroId: number): Promise<any> {
    const response = await axios.post(`${API_URL}/VaiTro/gan-vaitro`, {
      nguoiDungId: userId,
      vaiTroId: vaiTroId
    });
    return response.data;
  }

  /**
   * Gỡ vai trò khỏi người dùng.
   */
  async removeRole(userId: number, vaiTroId: number): Promise<any> {
    const response = await axios.post(`${API_URL}/VaiTro/go-vaitro`, {
      nguoiDungId: userId,
      vaiTroId: vaiTroId
    });
    return response.data;
  }

  /**
   * Gán kỹ năng cho người dùng.
   */
  async assignSkill(data: { nguoiDungId: number, kyNangId: number, level: number, soNamKinhNghiem: number }): Promise<any> {
    const response = await axios.post(`${API_URL}/NguoiDung/gan-kynang`, data);
    return response.data;
  }

  /**
   * Gỡ kỹ năng khỏi người dùng.
   */
  async removeSkill(data: { nguoiDungId: number, kyNangId: number }): Promise<any> {
    const response = await axios.post(`${API_URL}/NguoiDung/go-kynang`, data);
    return response.data;
  }
}

export default new UserService();
export * from './UserTypes';
