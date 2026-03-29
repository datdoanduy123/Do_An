using Apllication.DTOs;
using Apllication.IRepositories;
using Apllication.IService;

namespace Apllication.Service
{
    // Trien khai logic quan ly Nguoi dung
    public class NguoiDungService : INguoiDungService
    {
        private readonly INguoiDungRepository _nguoiDungRepo;

        public NguoiDungService(INguoiDungRepository nguoiDungRepo)
        {
            _nguoiDungRepo = nguoiDungRepo;
        }

        public async Task<NguoiDungDto?> TaoNguoiDungAsync(TaoNguoiDungDto taoNguoiDungDto)
        {
            // Validate: Kiem tra trung ten dang nhap hoac email
            var tonTai = await _nguoiDungRepo.KiemTraTonTaiAsync(taoNguoiDungDto.TenDangNhap, taoNguoiDungDto.Email);
            if (tonTai) return null;

            // Goi Repo thuc hien luu va map DTO
            return await _nguoiDungRepo.TaoNguoiDungAsync(taoNguoiDungDto);
        }

        public async Task<KetQuaPhanTrangDto<NguoiDungDto>> LayDanhSachNguoiDungAsync(NguoiDungQueryDto query)
        {
            return await _nguoiDungRepo.LayDanhSachNguoiDungAsync(query);
        }

        public async Task<NguoiDungDto?> LayTheoIdAsync(int id)
        {
            var user = await _nguoiDungRepo.LayTheoIdAsync(id);
            if (user == null) return null;

            var quyens = await _nguoiDungRepo.LayDanhSachQuyenCuaNguoiDungAsync(id);

            return new NguoiDungDto
            {
                Id = user.Id,
                TenDangNhap = user.Username,
                HoTen = user.FullName,
                Email = user.Email,
                DienThoai = user.DienThoai,
                CreatedAt = user.CreatedAt,
                VaiTros = user.NguoiDungVaiTros.Select(uv => uv.VaiTro.MaVaiTro).ToList(), // Thống nhất dùng MaVaiTro
                Quyens = quyens,
                KyNangs = user.KyNangNguoiDungs.Select(kn => new UserSkillDto
                {
                    KyNangId = kn.KyNangId,
                    TenKyNang = kn.KyNang!.TenKyNang,
                    Level = kn.Level,
                    SoNamKinhNghiem = kn.SoNamKinhNghiem
                }).ToList()
            };
        }

        public async Task<bool> CapNhatAsync(int id, CapNhatNguoiDungDto dto)
        {
            return await _nguoiDungRepo.CapNhatAsync(id, dto);
        }

        public async Task<bool> XoaMemAsync(int id)
        {
            return await _nguoiDungRepo.XoaMemAsync(id);
        }

        public async Task<bool> GanKyNangAsync(GanKyNangDto dto)
        {
            return await _nguoiDungRepo.GanKyNangAsync(dto);
        }

        public async Task<bool> GoKyNangAsync(GanKyNangDto dto)
        {
            return await _nguoiDungRepo.GoKyNangAsync(dto);
        }
    }
}
