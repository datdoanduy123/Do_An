using Apllication.DTOs;

namespace Apllication.IRepositories
{
    // Giao dien Repository cho thuc the NhomQuyen
    public interface INhomQuyenRepository
    {
        Task<bool> KiemTraTenNhomTonTaiAsync(string tenNhom);
        Task<NhomQuyenDto> TaoNhomQuyenAsync(TaoNhomQuyenDto taoNhomQuyenDto);
        Task<KetQuaPhanTrangDto<NhomQuyenDto>> LayDanhSachNhomQuyenAsync(NhomQuyenQueryDto query);
        Task<NhomQuyenDto?> LayTheoIdAsync(int id);
        Task<bool> CapNhatAsync(int id, CapNhatNhomQuyenDto dto);
        Task<bool> XoaAsync(int id);
    }
}
