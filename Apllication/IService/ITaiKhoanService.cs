using Apllication.DTOs;

namespace Apllication.IService
{
    // Giao dien dich vu Tai khoan (Dang nhap)
    public interface ITaiKhoanService
    {
        Task<NguoiDungDto?> DangNhapAsync(DangNhapDto dangNhapDto);
    }
}
