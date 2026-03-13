import axios from 'axios';
import type {
  QuyenDto,
  NhomQuyenDto,
  PaginatedResult,
  PermissionQuery
} from './PermissionTypes';

const API_URL = 'http://localhost:5095/api';

/**
 * Service xử lý các thao tác liên quan đến Quyền và Nhóm Quyền.
 */
class PermissionService {
  /**
   * Lấy danh sách nhóm quyền (để hiển thị trong dropdown hoặc bảng).
   */
  async getGroups(query: PermissionQuery = {}): Promise<PaginatedResult<NhomQuyenDto>> {
    const response = await axios.get(`${API_URL}/NhomQuyen/danh-sach`, { params: query });
    return response.data.data;
  }

  /**
   * Lấy danh sách quyền (có phân trang và tìm kiếm).
   */
  async getPermissions(query: PermissionQuery = {}): Promise<PaginatedResult<QuyenDto>> {
    const response = await axios.get(`${API_URL}/Quyen/danh-sach`, { params: query });
    return response.data.data;
  }

  /**
   * Xóa một quyền theo ID.
   */
  async deletePermission(id: number): Promise<boolean> {
    const response = await axios.delete(`${API_URL}/Quyen/${id}`);
    return response.data.statusCode === 200;
  }

  /**
   * Tạo quyền mới.
   */
  async createPermission(data: any): Promise<any> {
    const response = await axios.post(`${API_URL}/Quyen/tao-quyen`, data);
    return response.data;
  }

  /**
   * Cập nhật thông tin quyền.
   */
  async updatePermission(id: number, data: any): Promise<any> {
    const response = await axios.put(`${API_URL}/Quyen/${id}`, data);
    return response.data;
  }
}

export default new PermissionService();
export * from './PermissionTypes';
