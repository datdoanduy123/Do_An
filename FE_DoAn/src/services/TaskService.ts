import api from './api';
import type { CongViecDto, TaoCongViecDto } from './TaskTypes';

/**
 * Service xử lý các thao tác liên quan đến Công việc.
 */
class TaskService {
  /**
   * Lấy danh sách công việc theo ID dự án.
   */
  async getByProjectId(projectId: number): Promise<CongViecDto[]> {
    const response = await api.get(`/CongViec/du-an/${projectId}`);
    return response.data.data;
  }

  /**
   * Lấy danh sách công việc của người dùng hiện tại.
   */
  async getMyTasks(): Promise<CongViecDto[]> {
    const response = await api.get('/CongViec/my-tasks');
    return response.data.data;
  }

  /**
   * Lấy chi tiết công việc.
   */
  async getById(id: number): Promise<CongViecDto> {
    const response = await api.get(`/CongViec/${id}`);
    return response.data.data;
  }

  /**
   * Tạo công việc mới.
   */
  async create(data: TaoCongViecDto): Promise<any> {
    const response = await api.post('/CongViec/tao-cong-viec', data);
    return response.data;
  }

  /**
   * Cập nhật trạng thái công việc.
   */
  async updateStatus(id: number, status: number): Promise<boolean> {
    const response = await api.patch(`/CongViec/${id}/cap-nhat-trang-thai?status=${status}`);
    return response.data.statusCode === 200;
  }

  /**
   * Cập nhật tiến độ công việc (thời gian làm việc và trạng thái).
   */
  async updateProgress(id: number, data: { trangThai: number, thoiGianLamViecThem: number, ghiChu?: string }): Promise<boolean> {
    const response = await api.put(`/CongViec/${id}/cap-nhat-tien-do`, data);
    return response.data.statusCode === 200;
  }

  /**
   * Lấy danh sách công việc đang chờ duyệt (Review).
   */
  async getPendingReviews(): Promise<CongViecDto[]> {
    const response = await api.get('/CongViec/pending-reviews');
    return response.data.data;
  }

  /**
   * Giao việc cho nhân viên.
   */
  async assignTask(data: { congViecId: number, assigneeId: number }): Promise<boolean> {
    const response = await api.post('/CongViec/giao-viec-thu-cong', data);
    return response.data.statusCode === 200;
  }
}

export default new TaskService();
export * from './TaskTypes';
