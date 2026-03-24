using System.Threading.Tasks;

namespace Apllication.IService
{
    /// <summary>
    /// Interface định nghĩa dịch vụ thông báo Realtime cho Kanban board
    /// </summary>
    public interface IKanbanNotificationService
    {
        /// <summary>
        /// Thông báo cho các thành viên trong dự án rằng danh sách công việc đã thay đổi
        /// </summary>
        /// <param name="projectId">ID của dự án</param>
        Task NotifyTaskUpdated(int projectId);

        /// <summary>
        /// Gửi thông báo trực tiếp cho một người dùng cụ thể
        /// </summary>
        Task NotifyPersonal(int userId, string title, string message);
    }
}
