using Apllication.DTOs;

namespace Apllication.IService
{
    // Giao dien dich vu cho Tai khoan
    public interface IDichVuTaiKhoan
    {
        Task<NguoiDungDto?> DangNhapAsync(DangNhapDto dangNhapDto);
    }
}
