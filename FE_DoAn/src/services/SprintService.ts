import api from './api';
import type { SprintDto, TaoSprintDto, CapNhatSprintDto } from './SprintTypes';

/**
 * Service xử lý các thao tác liên quan đến Sprint.
 */
class SprintService {
  /**
   * Lấy danh sách sprint theo ID dự án.
   */
  async getByProjectId(projectId: number): Promise<SprintDto[]> {
    const response = await api.get(`/Sprint/du-an/${projectId}`);
    return response.data.data;
  }

  /**
   * Lấy chi tiết sprint.
   */
  async getById(id: number): Promise<SprintDto> {
    const response = await api.get(`/Sprint/${id}`);
    return response.data.data;
  }

  /**
   * Tạo sprint mới.
   */
  async create(data: TaoSprintDto): Promise<any> {
    const response = await api.post(`/Sprint/tao-sprint`, data);
    return response.data;
  }

  /**
   * Cập nhật sprint.
   */
  async update(id: number, data: CapNhatSprintDto): Promise<any> {
    const response = await api.put(`/Sprint/${id}`, data);
    return response.data;
  }

  /**
   * Xóa sprint.
   */
  async delete(id: number): Promise<boolean> {
    const response = await api.delete(`/Sprint/${id}`);
    return response.data.statusCode === 200;
  }
}

export default new SprintService();
export * from './SprintTypes';
