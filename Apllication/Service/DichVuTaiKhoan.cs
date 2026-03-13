using Apllication.DTOs;
using Apllication.IRepositories;
using Apllication.IService;

namespace Apllication.Service
{
    // Trien khai logic nghiep vu cho Tai khoan
    public class DichVuTaiKhoan : IDichVuTaiKhoan
    {
        private readonly INguoiDungRepository _nguoiDungRepo;
        private readonly IDichVuToken _tokenService;

        public DichVuTaiKhoan(INguoiDungRepository nguoiDungRepo, IDichVuToken tokenService)
        {
            _nguoiDungRepo = nguoiDungRepo;
            _tokenService = tokenService;
        }

        public async Task<NguoiDungDto?> DangNhapAsync(DangNhapDto dangNhapDto)
        {
            // Logic validate
            if (string.IsNullOrEmpty(dangNhapDto.TenDangNhap) || string.IsNullOrEmpty(dangNhapDto.MatKhau))
            {
                return null;
            }

            // Truy van qua Repository
            var nguoiDung = await _nguoiDungRepo.LayTheoTenDangNhapAsync(dangNhapDto.TenDangNhap);

            if (nguoiDung == null) return null;

            // Kiem tra mat khau
            if (nguoiDung.PasswordHash != dangNhapDto.MatKhau) return null;

            // Lay danh sach vai tro
            var vaiTros = await _nguoiDungRepo.LayDanhSachMaVaiTroCuaNguoiDungAsync(nguoiDung.Id);

            // Tao Token va tra ve ket qua
            return new NguoiDungDto
            {
                TenDangNhap = nguoiDung.Username,
                HoTen = nguoiDung.FullName,
            };
        }
    }
}
