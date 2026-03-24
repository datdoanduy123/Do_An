using Apllication.IRepositories;
using Apllication.IService;
using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apllication.Service
{
    public class ThongBaoService : IThongBaoService
    {
        private readonly IThongBaoRepository _repo;

        public ThongBaoService(IThongBaoRepository repo)
        {
            _repo = repo;
        }

        public async Task<bool> DanhDauDaDocAsync(int thongBaoId)
        {
            return await _repo.MarkAsReadAsync(thongBaoId);
        }

        public async Task<bool> DanhDauTatCaDaDocAsync(int userId)
        {
            return await _repo.MarkAllAsReadAsync(userId);
        }

        public async Task<int> LaySoLuongChuaDocAsync(int userId)
        {
            return await _repo.GetUnreadCountAsync(userId);
        }

        public async Task<IEnumerable<ThongBao>> LayThongBaoTheoUserAsync(int userId)
        {
            return await _repo.GetByUserIdAsync(userId);
        }

        public async Task<bool> XoaTatCaThongBaoAsync(int userId)
        {
            return await _repo.DeleteAllAsync(userId);
        }
    }
}
