import api from './api';
import type { 
  KyNangDto, 
  TaoKyNangDto, 
  CapNhatKyNangDto, 
  SkillQuery 
} from './SkillTypes';
import type { PaginatedResult } from './PermissionTypes';

/**
 * Service xử lý các thao tác liên quan đến Kỹ năng.
 */
class SkillService {
  /**
   * Lấy danh sách kỹ năng.
   */
  async getSkills(query: SkillQuery = {}): Promise<PaginatedResult<KyNangDto>> {
    const response = await api.get(`/KyNang/danh-sach`, { params: query });
    return response.data.data;
  }

  /**
   * Lấy chi tiết kỹ năng.
   */
  async getSkillById(id: number): Promise<KyNangDto> {
    const response = await api.get(`/KyNang/${id}`);
    return response.data.data;
  }

  /**
   * Tạo kỹ năng mới.
   */
  async createSkill(data: TaoKyNangDto): Promise<any> {
    const response = await api.post(`/KyNang/tao-ky-nang`, data);
    return response.data;
  }

  /**
   * Cập nhật kỹ năng.
   */
  async updateSkill(id: number, data: CapNhatKyNangDto): Promise<any> {
    const response = await api.put(`/KyNang/${id}`, data);
    return response.data;
  }

  /**
   * Xóa kỹ năng.
   */
  async deleteSkill(id: number): Promise<boolean> {
    const response = await api.delete(`/KyNang/${id}`);
    return response.data.statusCode === 200;
  }
}

export default new SkillService();
export * from './SkillTypes';
