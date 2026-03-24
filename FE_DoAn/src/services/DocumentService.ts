import api from './api';
import type { TaiLieuDuAnDto } from './DocumentTypes';

/**
 * Service xử lý các thao tác liên quan đến Tài liệu dự án.
 */
class DocumentService {
  /**
   * Tải tài liệu lên cho một dự án.
   */
  async upload(projectId: number, file: File): Promise<any> {
    const formData = new FormData();
    formData.append('file', file);

    const response = await api.post(`/TaiLieuDuAn/upload/${projectId}`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  }

  /**
   * Lấy danh sách tài liệu của dự án.
   */
  async getByProject(projectId: number): Promise<TaiLieuDuAnDto[]> {
    const response = await api.get(`/TaiLieuDuAn/du-an/${projectId}`);
    return response.data.data;
  }

  /**
   * Kích hoạt AI để bóc tách công việc từ tài liệu.
   */
  async processAI(documentId: number): Promise<any> {
    const response = await api.post(`/TaiLieuDuAn/${documentId}/xu-ly-ai`);
    return response.data.statusCode === 200;
  }
}

export default new DocumentService();
export * from './DocumentTypes';
