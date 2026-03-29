import api from './api';

export interface AiRuleDto {
  id: number;
  maQuyTac: string;
  giaTri: string;
  loaiDuLieu: string;
  moTa?: string;
  isActive: boolean;
}

export interface UpdateAiRuleDto {
  giaTri: string;
  isActive: boolean;
}

const AiRuleService = {
  /**
   * Lấy danh sách tất cả quy tắc AI.
   */
  getAll: async (): Promise<AiRuleDto[]> => {
    const response = await api.get('/QuyTacGiaoViecAI');
    return response.data;
  },

  /**
   * Lấy chi tiết một quy tắc.
   */
  getById: async (id: number): Promise<AiRuleDto> => {
    const response = await api.get(`/QuyTacGiaoViecAI/${id}`);
    return response.data;
  },

  /**
   * Cập nhật quy tắc AI.
   */
  update: async (id: number, data: UpdateAiRuleDto): Promise<void> => {
    await api.put(`/QuyTacGiaoViecAI/${id}`, data);
  }
};

export default AiRuleService;
