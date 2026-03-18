import api from './api';

export interface AIRecommendation {
  userId: number;
  hoTen: string;
  diemPhuHop: number;
  lyDo: string;
  kyNangPhuHop: string[];
}

class GiaoViecAIService {
  async getRecommendations(taskId: number): Promise<AIRecommendation[]> {
    const response = await api.get(`/CongViec/${taskId}/goi-y-ai`);
    return response.data.data;
  }
}

export default new GiaoViecAIService();
