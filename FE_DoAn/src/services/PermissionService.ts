import api from './api';
import type {
  QuyenDto,
  NhomQuyenDto,
  PaginatedResult,
  PermissionQuery
} from './PermissionTypes';

/**
 * Service xử lý các thao tác liên quan đến Quyền và Nhóm Quyền.
 */
class PermissionService {
  /**
   * Lấy danh sách nhóm quyền (để hiển thị trong dropdown hoặc bảng).
   */
  async getGroups(query: PermissionQuery = {}): Promise<PaginatedResult<NhomQuyenDto>> {
    const response = await api.get(`/NhomQuyen/danh-sach`, { params: query });
    return response.data.data;
  }

  /**
   * Lấy danh sách quyền (có phân trang và tìm kiếm).
   */
  async getPermissions(query: PermissionQuery = {}): Promise<PaginatedResult<QuyenDto>> {
    const response = await api.get(`/Quyen/danh-sach`, { params: query });
    return response.data.data;
  }

  /**
   * Xóa một quyền theo ID.
   */
  async deletePermission(id: number): Promise<boolean> {
    const response = await api.delete(`/Quyen/${id}`);
    return response.data.statusCode === 200;
  }

  /**
   * Tạo quyền mới.
   */
  async createPermission(data: any): Promise<any> {
    const response = await api.post(`/Quyen/tao-quyen`, data);
    return response.data;
  }

  /**
   * Cập nhật thông tin quyền.
   */
  async updatePermission(id: number, data: any): Promise<any> {
    const response = await api.put(`/Quyen/${id}`, data);
    return response.data;
  }
}

export default new PermissionService();
export * from './PermissionTypes';
