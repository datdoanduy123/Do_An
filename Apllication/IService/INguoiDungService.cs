using Apllication.DTOs;

namespace Apllication.IService
{
    // Giao dien dich vu quan ly Nguoi dung
    public interface INguoiDungService
    {
        Task<NguoiDungDto?> TaoNguoiDungAsync(TaoNguoiDungDto taoNguoiDungDto);
        Task<KetQuaPhanTrangDto<NguoiDungDto>> LayDanhSachNguoiDungAsync(NguoiDungQueryDto query);
        Task<NguoiDungDto?> LayTheoIdAsync(int id);
        Task<bool> CapNhatAsync(int id, CapNhatNguoiDungDto dto);
        Task<bool> XoaMemAsync(int id);
    }
}
