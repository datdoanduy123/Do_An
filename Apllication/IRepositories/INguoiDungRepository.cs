using Apllication.DTOs;
using Domain.Entities;

namespace Apllication.IRepositories
{
    // Giao dien repository cho thuc the Nguoi dung
    public interface INguoiDungRepository
    {
        Task<User?> LayTheoTenDangNhapAsync(string tenDangNhap);
        Task<bool> KiemTraTonTaiAsync(string tenDangNhap, string email);
        Task<NguoiDungDto> TaoNguoiDungAsync(TaoNguoiDungDto taoNguoiDungDto);
        Task<List<string>> LayDanhSachQuyenCuaNguoiDungAsync(int userId);
        Task<List<string>> LayDanhSachMaVaiTroCuaNguoiDungAsync(int userId);
        Task<KetQuaPhanTrangDto<NguoiDungDto>> LayDanhSachNguoiDungAsync(NguoiDungQueryDto query);
        Task<User?> LayTheoIdAsync(int id);
        Task<bool> CapNhatAsync(int id, CapNhatNguoiDungDto dto);
        Task<bool> XoaMemAsync(int id);
    }
}
