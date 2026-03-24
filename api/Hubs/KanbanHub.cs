using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace api.Hubs
{
    /// <summary>
    /// Hub SignalR cho các thông báo liên quan đến Kanban board
    /// </summary>
    public class KanbanHub : Hub
    {
        /// <summary>
        /// Cho phép client tham gia vào một "phòng" dự án để nhận thông báo riêng của dự án đó
        /// </summary>
        /// <param name="projectId">ID dự án</param>
        public async Task JoinProject(int projectId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Project_{projectId}");
        }

        /// <summary>
        /// Cho phép client rời khỏi "phòng" dự án
        /// </summary>
        /// <param name="projectId">ID dự án</param>
        public async Task LeaveProject(int projectId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Project_{projectId}");
        }

        /// <summary>
        /// Tham gia vào phòng thông báo cá nhân
        /// </summary>
        public async Task JoinUser(int userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
        }
    }
}
