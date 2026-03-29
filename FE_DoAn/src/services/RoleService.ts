import api from './api';
import type { 
  VaiTroDto, 
  TaoVaiTroDto, 
  CapNhatVaiTroDto, 
  VaiTroQuery 
} from './RoleTypes';
import type { PaginatedResult } from './PermissionTypes';

/**
 * Service xử lý các thao tác liên quan đến Vai trò.
 */
class RoleService {
  /**
   * Lấy danh sách vai trò (có phân trang và tìm kiếm).
   */
  async getRoles(query: VaiTroQuery = {}): Promise<PaginatedResult<VaiTroDto>> {
    const response = await api.get(`/VaiTro/danh-sach`, { params: query });
    return response.data.data;
  }

  /**
   * Lấy chi tiết vai trò theo ID.
   */
  async getRoleById(id: number): Promise<VaiTroDto> {
    const response = await api.get(`/VaiTro/${id}`);
    return response.data.data;
  }

  /**
   * Tạo vai trò mới.
   */
  async createRole(data: TaoVaiTroDto): Promise<any> {
    const response = await api.post(`/VaiTro/tao-vaitro`, data);
    return response.data;
  }

  /**
   * Cập nhật thông tin vai trò.
   */
  async updateRole(id: number, data: CapNhatVaiTroDto): Promise<any> {
    const response = await api.put(`/VaiTro/${id}`, data);
    return response.data;
  }

  /**
   * Xóa một vai trò theo ID.
   */
  async deleteRole(id: number): Promise<boolean> {
    const response = await api.delete(`/VaiTro/${id}`);
    return response.data.statusCode === 200;
  }

  /**
   * Lấy danh sách quyền của một vai trò.
   */
  async getPermissionsByRole(roleId: number): Promise<any[]> {
    const response = await api.get(`/VaiTro/${roleId}/quyens`);
    return response.data.data;
  }

  /**
   * Gán quyền cho vai trò.
   */
  async assignPermission(roleId: number, quyenId: number): Promise<any> {
    const response = await api.post(`/VaiTro/gan-quyen-cho-vaitro`, {
      vaiTroId: roleId,
      quyenId: quyenId
    });
    return response.data;
  }

  /**
   * Gỡ quyền khỏi vai trò.
   */
  async removePermission(roleId: number, quyenId: number): Promise<any> {
    const response = await api.post(`/VaiTro/go-quyen`, {
      vaiTroId: roleId,
      quyenId: quyenId
    });
    return response.data;
  }
}

export default new RoleService();
export * from './RoleTypes';
