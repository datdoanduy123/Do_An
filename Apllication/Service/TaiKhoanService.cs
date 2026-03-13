using Apllication.DTOs;
using Apllication.IRepositories;
using Apllication.IService;

namespace Apllication.Service
{
    // Trien khai logic Tai khoan
    public class TaiKhoanService : ITaiKhoanService
    {
        private readonly INguoiDungRepository _nguoiDungRepo;
        private readonly IDichVuToken _tokenService;
        private readonly IMatKhauService _matKhauService;

        public TaiKhoanService(INguoiDungRepository nguoiDungRepo, IDichVuToken tokenService, IMatKhauService matKhauService)
        {
            _nguoiDungRepo = nguoiDungRepo;
            _tokenService = tokenService;
            _matKhauService = matKhauService;
        }

        public async Task<NguoiDungDto?> DangNhapAsync(DangNhapDto dangNhapDto)
        {
            var nguoiDung = await _nguoiDungRepo.LayTheoTenDangNhapAsync(dangNhapDto.TenDangNhap);

            if (nguoiDung == null || !_matKhauService.XacMinhPassword(dangNhapDto.MatKhau, nguoiDung.PasswordHash))
            {
                return null;
            }

            // Lay danh sach vai tro cua nguoi dung
            var vaiTroMas = await _nguoiDungRepo.LayDanhSachMaVaiTroCuaNguoiDungAsync(nguoiDung.Id);

            return new NguoiDungDto
            {
                Id = nguoiDung.Id,
                TenDangNhap = nguoiDung.Username,
                HoTen = nguoiDung.FullName,
                Email = nguoiDung.Email,
                DienThoai = nguoiDung.DienThoai,
                CreatedAt = nguoiDung.CreatedAt,
                VaiTros = nguoiDung.NguoiDungVaiTros.Select(uv => uv.VaiTro.TenVaiTro).ToList()            };
        }
    }
}
