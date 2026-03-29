import api from './api';
import type { 
  KyNangDto, 
  TaoKyNangDto, 
  CapNhatKyNangDto, 
  SkillQuery,
  NhomKyNangDto,
  CongNgheDto,
  TaoNhomKyNangDto,
  TaoCongNgheDto
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

  /**
   * Lấy toàn bộ phân cấp kỹ năng.
   */
  async getHierarchy(): Promise<NhomKyNangDto[]> {
    const response = await api.get(`/KyNang/hierarchy`);
    return response.data.data;
  }

  /**
   * Lấy danh sách nhóm kỹ năng.
   */
  async getNhomKyNangs(): Promise<NhomKyNangDto[]> {
    const response = await api.get(`/KyNang/nhom-danh-sach`);
    return response.data.data;
  }

  /**
   * Lấy danh sách công nghệ theo nhóm.
   */
  async getCongNgheByNhom(nhomId: number): Promise<CongNgheDto[]> {
    const response = await api.get(`/KyNang/cong-nghe-theo-nhom/${nhomId}`);
    return response.data.data;
  }

  /**
   * Tạo nhóm kỹ năng mới.
   */
  async createNhom(data: TaoNhomKyNangDto): Promise<NhomKyNangDto> {
    const response = await api.post(`/KyNang/tao-nhom`, data);
    return response.data.data;
  }

  /**
   * Tạo công nghệ mới.
   */
  async createCongNghe(data: TaoCongNgheDto): Promise<CongNgheDto> {
    const response = await api.post(`/KyNang/tao-cong-nghe`, data);
    return response.data.data;
  }
}

export default new SkillService();
export * from './SkillTypes';
