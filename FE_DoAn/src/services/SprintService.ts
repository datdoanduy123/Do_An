import axios from 'axios';
import type { SprintDto, TaoSprintDto, CapNhatSprintDto } from './SprintTypes';

const API_URL = 'http://localhost:5095/api';

/**
 * Service xử lý các thao tác liên quan đến Sprint.
 */
class SprintService {
  /**
   * Lấy danh sách sprint theo ID dự án.
   */
  async getByProjectId(projectId: number): Promise<SprintDto[]> {
    const response = await axios.get(`${API_URL}/Sprint/du-an/${projectId}`);
    return response.data.data;
  }

  /**
   * Lấy chi tiết sprint.
   */
  async getById(id: number): Promise<SprintDto> {
    const response = await axios.get(`${API_URL}/Sprint/${id}`);
    return response.data.data;
  }

  /**
   * Tạo sprint mới.
   */
  async create(data: TaoSprintDto): Promise<any> {
    const response = await axios.post(`${API_URL}/Sprint/tao-sprint`, data);
    return response.data;
  }

  /**
   * Cập nhật sprint.
   */
  async update(id: number, data: CapNhatSprintDto): Promise<any> {
    const response = await axios.put(`${API_URL}/Sprint/${id}`, data);
    return response.data;
  }

  /**
   * Xóa sprint.
   */
  async delete(id: number): Promise<boolean> {
    const response = await axios.delete(`${API_URL}/Sprint/${id}`);
    return response.data.statusCode === 200;
  }
}

export default new SprintService();
export * from './SprintTypes';
