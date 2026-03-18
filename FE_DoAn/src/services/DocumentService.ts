import api from './api';

export interface DocumentDto {
  id: number;
  duAnId: number;
  fileName: string;
  filePath: string;
  fileType: string;
  uploadedBy: number;
  uploadAt: string;
  isProcessed: boolean;
}

class DocumentService {
  async getByProject(projectId: number): Promise<DocumentDto[]> {
    const response = await api.get(`/TaiLieuDuAn/du-an/${projectId}`);
    return response.data.data;
  }

  async upload(projectId: number, file: File): Promise<DocumentDto> {
    const formData = new FormData();
    formData.append('file', file);

    const response = await api.post(`/TaiLieuDuAn/upload/${projectId}`, formData, {
      headers: { 
        'Content-Type': 'multipart/form-data'
      }
    });
    return response.data.data;
  }

  async processAI(documentId: number): Promise<boolean> {
    const response = await api.post(`/TaiLieuDuAn/${documentId}/xu-ly-ai`);
    return response.data.statusCode === 200;
  }
}

export default new DocumentService();
