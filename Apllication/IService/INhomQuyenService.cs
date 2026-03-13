using Apllication.DTOs;

namespace Apllication.IService
{
    // Giao dien Service cho thuc the NhomQuyen
    public interface INhomQuyenService
    {
        Task<NhomQuyenDto> TaoNhomQuyenAsync(TaoNhomQuyenDto taoNhomQuyenDto);
        Task<KetQuaPhanTrangDto<NhomQuyenDto>> LayDanhSachNhomQuyenAsync(NhomQuyenQueryDto query);
        Task<NhomQuyenDto?> LayTheoIdAsync(int id);
        Task<bool> CapNhatAsync(int id, CapNhatNhomQuyenDto dto);
        Task<bool> XoaAsync(int id);
    }
}
