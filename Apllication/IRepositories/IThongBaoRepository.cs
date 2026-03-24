using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apllication.IRepositories
{
    public interface IThongBaoRepository
    {
        Task<IEnumerable<ThongBao>> GetByUserIdAsync(int userId, int limit = 20);
        Task<int> GetUnreadCountAsync(int userId);
        Task<bool> MarkAsReadAsync(int notificationId);
        Task<bool> MarkAllAsReadAsync(int userId);
        Task<bool> DeleteAllAsync(int userId);
        Task<bool> AddAsync(ThongBao thongBao);
    }
}
