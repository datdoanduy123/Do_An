using Apllication.DTOs;

namespace Apllication.IRepositories
{
    // Giao dien Repository cho thuc the Quyen
    public interface IQuyenRepository
    {
        Task<bool> KiemTraMaQuyenTonTaiAsync(string maQuyen);
        Task<QuyenDto> TaoQuyenAsync(TaoQuyenDto taoQuyenDto);
        Task<KetQuaPhanTrangDto<QuyenDto>> LayDanhSachQuyenAsync(QuyenQueryDto query);
        Task<QuyenDto?> LayTheoIdAsync(int id);
        Task<bool> CapNhatAsync(int id, CapNhatQuyenDto dto);
        Task<bool> XoaAsync(int id);
    }
}
