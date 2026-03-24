using Apllication.DTOs;
using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apllication.IService
{
    public interface IThongBaoService
    {
        Task<IEnumerable<ThongBao>> LayThongBaoTheoUserAsync(int userId);
        Task<int> LaySoLuongChuaDocAsync(int userId);
        Task<bool> DanhDauDaDocAsync(int thongBaoId);
        Task<bool> DanhDauTatCaDaDocAsync(int userId);
        Task<bool> XoaTatCaThongBaoAsync(int userId);
    }
}
