using Apllication.IService;
using api.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Apllication.IRepositories;

namespace api.Services
{
    /// <summary>
    /// Triển khai dịch vụ thông báo Kanban sử dụng SignalR
    /// </summary>
    public class KanbanNotificationService : IKanbanNotificationService
    {
        private readonly IHubContext<KanbanHub> _hubContext;
        private readonly IThongBaoRepository _thongBaoRepo;

        public KanbanNotificationService(IHubContext<KanbanHub> hubContext, IThongBaoRepository thongBaoRepo)
        {
            _hubContext = hubContext;
            _thongBaoRepo = thongBaoRepo;
        }

        /// <summary>
        /// Gửi thông báo 'TaskUpdated' tới tất cả client trong nhóm của dự án cụ thể
        /// </summary>
        public async Task NotifyTaskUpdated(int projectId)
        {
            // Gửi tới tất cả các kết nối trong Group "Project_{projectId}"
            await _hubContext.Clients.Group($"Project_{projectId}").SendAsync("TaskUpdated", projectId);
        }

        public async Task NotifyPersonal(int userId, string title, string message)
        {
            // 1. Lưu vào Database
            var thongBao = new Domain.Entities.ThongBao
            {
                UserId = userId,
                Title = title,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            await _thongBaoRepo.AddAsync(thongBao);

            // 2. Gửi Realtime qua Hub
            await _hubContext.Clients.Group($"User_{userId}").SendAsync("ReceiveNotification", new { 
                id = thongBao.Id, // Gửi kèm Id từ DB để Frontend có thể đánh dấu đã đọc
                title, 
                message 
            });
        }
    }
}
