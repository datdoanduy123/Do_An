import axios from 'axios';
import type { 
  VaiTroDto, 
  TaoVaiTroDto, 
  CapNhatVaiTroDto, 
  VaiTroQuery 
} from './RoleTypes';
import type { PaginatedResult } from './PermissionTypes';

const API_URL = 'http://localhost:5095/api';

/**
 * Service xử lý các thao tác liên quan đến Vai trò.
 */
class RoleService {
  /**
   * Lấy danh sách vai trò (có phân trang và tìm kiếm).
   */
  async getRoles(query: VaiTroQuery = {}): Promise<PaginatedResult<VaiTroDto>> {
    const response = await axios.get(`${API_URL}/VaiTro/danh-sach`, { params: query });
    return response.data.data;
  }

  /**
   * Lấy chi tiết vai trò theo ID.
   */
  async getRoleById(id: number): Promise<VaiTroDto> {
    const response = await axios.get(`${API_URL}/VaiTro/${id}`);
    return response.data.data;
  }

  /**
   * Tạo vai trò mới.
   */
  async createRole(data: TaoVaiTroDto): Promise<any> {
    const response = await axios.post(`${API_URL}/VaiTro/tao-vaitro`, data);
    return response.data;
  }

  /**
   * Cập nhật thông tin vai trò.
   */
  async updateRole(id: number, data: CapNhatVaiTroDto): Promise<any> {
    const response = await axios.put(`${API_URL}/VaiTro/${id}`, data);
    return response.data;
  }

  /**
   * Xóa một vai trò theo ID.
   */
  async deleteRole(id: number): Promise<boolean> {
    const response = await axios.delete(`${API_URL}/VaiTro/${id}`);
    return response.data.statusCode === 200;
  }
}

export default new RoleService();
export * from './RoleTypes';
