import api from './api';

export interface Notification {
  id: number;
  userId: number;
  title: string;
  message: string;
  isRead: boolean;
  createdAt: string;
  projectId?: number;
}

const NotificationService = {
  /**
   * Lấy danh sách thông báo của người dùng
   */
  getByUser: async (userId: number): Promise<Notification[]> => {
    const response = await api.get(`/thongbao/user/${userId}`);
    return response.data;
  },

  /**
   * Lấy số lượng thông báo chưa đọc
   */
  getUnreadCount: async (userId: number): Promise<number> => {
    const response = await api.get(`/thongbao/unread-count/${userId}`);
    return response.data;
  },

  /**
   * Đánh dấu một thông báo đã đọc
   */
  markRead: async (id: number): Promise<boolean> => {
    const response = await api.post(`/thongbao/mark-read/${id}`);
    return response.data;
  },

  /**
   * Đánh dấu tất cả thông báo của người dùng là đã đọc
   */
  markAllRead: async (userId: number): Promise<boolean> => {
    const response = await api.post(`/thongbao/mark-all-read/${userId}`);
    return response.data;
  },

  /**
   * Xóa tất cả thông báo của người dùng
   */
  deleteAll: async (userId: number): Promise<boolean> => {
    const response = await api.delete(`/thongbao/all/${userId}`);
    return response.data;
  }
};

export default NotificationService;
