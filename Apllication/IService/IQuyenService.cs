using Apllication.DTOs;

namespace Apllication.IService
{
    // Giao dien Service cho thuc the Quyen
    public interface IQuyenService
    {
        Task<QuyenDto> TaoQuyenAsync(TaoQuyenDto taoQuyenDto);
        Task<KetQuaPhanTrangDto<QuyenDto>> LayDanhSachQuyenAsync(QuyenQueryDto query);
        Task<QuyenDto?> LayTheoIdAsync(int id);
        Task<bool> CapNhatAsync(int id, CapNhatQuyenDto dto);
        Task<bool> XoaAsync(int id);
    }
}
