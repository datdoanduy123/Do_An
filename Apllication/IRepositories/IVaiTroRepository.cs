using Apllication.DTOs;

namespace Apllication.IRepositories
{
    // Giao dien Repository cho thuc the VaiTro
    public interface IVaiTroRepository
    {
        Task<bool> KiemTraTenVaiTroTonTaiAsync(string tenVaiTro);
        Task<VaiTroDto> TaoVaiTroAsync(TaoVaiTroDto taoVaiTroDto);
        Task<bool> GanVaiTroChoNguoiDungAsync(GanVaiTroDto ganVaiTroDto);
        Task<bool> GanQuyenChoVaiTroAsync(GanQuyenChoVaiTroDto ganQuyenDto);
        Task<KetQuaPhanTrangDto<VaiTroDto>> LayDanhSachVaiTroAsync(VaiTroQueryDto query);
        Task<VaiTroDto?> LayTheoIdAsync(int id);
        Task<bool> CapNhatAsync(int id, CapNhatVaiTroDto dto);
        Task<bool> XoaAsync(int id);
        Task<bool> GoVaiTroKhoiNguoiDungAsync(GanVaiTroDto dto);
        Task<bool> GoQuyenKhoiVaiTroAsync(GanQuyenChoVaiTroDto dto);
        Task<List<QuyenDto>> LayDanhSachQuyenTheoVaiTroAsync(int vaiTroId);
    }
}
