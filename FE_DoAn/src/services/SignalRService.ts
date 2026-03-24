import * as signalR from '@microsoft/signalr';

/**
 * Service quản lý kết nối SignalR Realtime.
 */
class SignalRService {
  private hubConnection: signalR.HubConnection | null = null;
  private readonly hubUrl: string;

  constructor() {
    // API URL từ biến môi trường hoặc mặc định
    const baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:5095';
    this.hubUrl = `${baseUrl}/hubs/kanban`;
  }

  /**
   * Khởi tạo kết nối tới Hub
   */
  async startConnection(): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) return;

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets
      })
      .withAutomaticReconnect()
      .build();

    try {
      await this.hubConnection.start();
      console.log('SignalR connected successfully.');
    } catch (err) {
      console.error('SignalR connection failed: ', err);
      // Thử lại sau 5s nếu lỗi
      setTimeout(() => this.startConnection(), 5000);
    }
  }

  /**
   * Lắng nghe sự kiện từ Server
   */
  on(eventName: string, callback: (...args: any[]) => void) {
    this.hubConnection?.on(eventName, callback);
  }

  /**
   * Hủy lắng nghe sự kiện
   */
  off(eventName: string) {
    this.hubConnection?.off(eventName);
  }

  /**
   * Tham gia vào nhóm dự án cụ thể
   */
  async joinProject(projectId: number) {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('JoinProject', projectId);
    }
  }

  /**
   * Tham gia vào phòng thông báo cá nhân
   */
  async joinUser(userId: number) {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('JoinUser', userId);
    }
  }

  /**
   * Rời khỏi nhóm dự án
   */
  async leaveProject(projectId: number) {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('LeaveProject', projectId);
    }
  }
}

export default new SignalRService();
