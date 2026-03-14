import axios from 'axios';
import type { 
  KyNangDto, 
  TaoKyNangDto, 
  CapNhatKyNangDto, 
  SkillQuery 
} from './SkillTypes';
import type { PaginatedResult } from './PermissionTypes';

const API_URL = 'http://localhost:5095/api';

/**
 * Service xử lý các thao tác liên quan đến Kỹ năng.
 */
class SkillService {
  /**
   * Lấy danh sách kỹ năng.
   */
  async getSkills(query: SkillQuery = {}): Promise<PaginatedResult<KyNangDto>> {
    const response = await axios.get(`${API_URL}/KyNang/danh-sach`, { params: query });
    return response.data.data;
  }

  /**
   * Lấy chi tiết kỹ năng.
   */
  async getSkillById(id: number): Promise<KyNangDto> {
    const response = await axios.get(`${API_URL}/KyNang/${id}`);
    return response.data.data;
  }

  /**
   * Tạo kỹ năng mới.
   */
  async createSkill(data: TaoKyNangDto): Promise<any> {
    const response = await axios.post(`${API_URL}/KyNang/tao-ky-nang`, data);
    return response.data;
  }

  /**
   * Cập nhật kỹ năng.
   */
  async updateSkill(id: number, data: CapNhatKyNangDto): Promise<any> {
    const response = await axios.put(`${API_URL}/KyNang/${id}`, data);
    return response.data;
  }

  /**
   * Xóa kỹ năng.
   */
  async deleteSkill(id: number): Promise<boolean> {
    const response = await axios.delete(`${API_URL}/KyNang/${id}`);
    return response.data.statusCode === 200;
  }
}

export default new SkillService();
export * from './SkillTypes';
