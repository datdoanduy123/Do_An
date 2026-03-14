using Apllication.DTOs;

namespace Apllication.IService
{
    public interface IKyNangService
    {
        Task<KetQuaPhanTrangDto<KyNangDto>> LayDanhSachKyNangAsync(KyNangQueryDto query);
        Task<KyNangDto?> LayTheoIdAsync(int id);
        Task<KyNangDto> TaoKyNangAsync(TaoKyNangDto dto);
        Task<bool> CapNhatAsync(int id, CapNhatKyNangDto dto);
        Task<bool> XoaAsync(int id);
    }
}
