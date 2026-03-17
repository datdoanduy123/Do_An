import api from './api';
import type { DuAnDto, TaoDuAnDto, CapNhatDuAnDto } from './ProjectTypes';

/**
 * Service xử lý các thao tác liên quan đến Dự án.
 */
class ProjectService {
  /**
   * Lấy danh sách dự án.
   */
  async getProjects(): Promise<DuAnDto[]> {
    const response = await api.get('/DuAn/danh-sach');
    return response.data.data;
  }

  /**
   * Lấy chi tiết dự án.
   */
  async getProjectById(id: number): Promise<DuAnDto> {
    const response = await api.get(`/DuAn/${id}`);
    return response.data.data;
  }

  /**
   * Tạo dự án mới.
   */
  async createProject(data: TaoDuAnDto): Promise<any> {
    const response = await api.post('/DuAn/tao-du-an', data);
    return response.data;
  }

  /**
   * Cập nhật dự án.
   */
  async updateProject(id: number, data: CapNhatDuAnDto): Promise<any> {
    const response = await api.put(`/DuAn/${id}`, data);
    return response.data;
  }

  /**
   * Xóa dự án (xóa mềm hoặc xóa vĩnh viễn tùy backend).
   */
  async deleteProject(id: number): Promise<boolean> {
    const response = await api.delete(`/DuAn/${id}`);
    return response.data.statusCode === 200;
  }
}

export default new ProjectService();
export * from './ProjectTypes';
