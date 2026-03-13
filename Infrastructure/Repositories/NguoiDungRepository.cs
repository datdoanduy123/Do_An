using Apllication.DTOs;
using Apllication.IRepositories;
using Apllication.IService;
using Domain.Entities;
using Infrastructure.Helpers;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    // Trien khai repository cho thuc the Nguoi dung
    public class NguoiDungRepository : INguoiDungRepository
    {
        private readonly AppDbContext _boiCanh;
        private readonly IMatKhauService _matKhauService;

        public NguoiDungRepository(AppDbContext boiCanh, IMatKhauService matKhauService)
        {
            _boiCanh = boiCanh;
            _matKhauService = matKhauService;
        }

        public async Task<User?> LayTheoTenDangNhapAsync(string tenDangNhap)
        {
            return await _boiCanh.Users
                .Include(u => u.NguoiDungVaiTros)
                .ThenInclude(uv => uv.VaiTro)
                .SingleOrDefaultAsync(x => x.Username == tenDangNhap);
        }

        public async Task<bool> KiemTraTonTaiAsync(string tenDangNhap, string email)
        {
            return await _boiCanh.Users.AnyAsync(x => x.Username == tenDangNhap || x.Email == email);
        }

        public async Task<NguoiDungDto> TaoNguoiDungAsync(TaoNguoiDungDto taoNguoiDungDto)
        {
            var nguoiDung = new User
            {
                Username = taoNguoiDungDto.TenDangNhap,
                PasswordHash = _matKhauService.TaoPasswordHash(taoNguoiDungDto.MatKhau),
                FullName = taoNguoiDungDto.HoTen,
                Email = taoNguoiDungDto.Email,
                DienThoai = taoNguoiDungDto.DienThoai,
                VaiTro = "User",
                CreatedAt = DateTime.UtcNow
            };

            _boiCanh.Users.Add(nguoiDung);
            await _boiCanh.SaveChangesAsync();

            // Mapping DTO ngay tai Repository theo yeu cau
            return new NguoiDungDto
            {
                Id = nguoiDung.Id,
                TenDangNhap = nguoiDung.Username,
                HoTen = nguoiDung.FullName,
                Email = nguoiDung.Email,
                DienThoai = nguoiDung.DienThoai,
                CreatedAt = nguoiDung.CreatedAt            };
        }
        public async Task<List<string>> LayDanhSachQuyenCuaNguoiDungAsync(int userId)
        {
            var user = await _boiCanh.Users
                .Include(u => u.NguoiDungVaiTros)
                .ThenInclude(uv => uv.VaiTro)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return new List<string>();

            // Lay tat ca ma vai tro cua user
            var maVaiTros = user.NguoiDungVaiTros.Select(x => x.VaiTro.MaVaiTro).ToList();

            // Neu la QUAN_LY -> Lay tat ca quyen trong he thong
            if (maVaiTros.Contains("QUAN_LY"))
            {
                return await _boiCanh.Quyens.Select(x => x.MaQuyen).ToListAsync();
            }

            // Neu la NhanVien -> Lay quyen theo phan quyen trong bang VaiTroQuyen
            var vaiTroIds = user.NguoiDungVaiTros.Select(x => x.VaiTroId).ToList();
            
            var quyens = await _boiCanh.VaiTroQuyens
                .Where(vq => vaiTroIds.Contains(vq.VaiTroId))
                .Select(vq => vq.Quyen.MaQuyen)
                .Distinct()
                .ToListAsync();

            return quyens;
        }

        public async Task<List<string>> LayDanhSachMaVaiTroCuaNguoiDungAsync(int userId)
        {
            var user = await _boiCanh.Users
                .Include(u => u.NguoiDungVaiTros)
                .ThenInclude(uv => uv.VaiTro)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return new List<string>();

            return user.NguoiDungVaiTros.Select(x => x.VaiTro.MaVaiTro).ToList();
        }

        public async Task<KetQuaPhanTrangDto<NguoiDungDto>> LayDanhSachNguoiDungAsync(NguoiDungQueryDto query)
        {
            var queryable = _boiCanh.Users
                .Where(u => u.IsActive) // Chi lay nguoi dung dang hoat dong
                .Include(u => u.NguoiDungVaiTros)
                .ThenInclude(uv => uv.VaiTro)
                .AsQueryable();

            // Loc theo tu khoa
            if (!string.IsNullOrEmpty(query.Keyword))
            {
                var k = query.Keyword.ToLower();
                queryable = queryable.Where(x => x.FullName.ToLower().Contains(k) || x.Username.ToLower().Contains(k));
            }

            // Mix logic select sang DTO va phan trang
            return await queryable
                .Select(x => new NguoiDungDto
                {
                    Id = x.Id,
                    TenDangNhap = x.Username,
                    HoTen = x.FullName,
                    Email = x.Email,
                    DienThoai = x.DienThoai,
                    CreatedAt = x.CreatedAt,
                    VaiTros = x.NguoiDungVaiTros.Select(uv => uv.VaiTro.TenVaiTro).ToList()                })
                .ToPagedListAsync(query.PageIndex, query.PageSize);
        }

        public async Task<User?> LayTheoIdAsync(int id)
        {
            return await _boiCanh.Users
                .Include(u => u.NguoiDungVaiTros)
                .ThenInclude(uv => uv.VaiTro)
                .FirstOrDefaultAsync(u => u.Id == id && u.IsActive);
        }

        public async Task<bool> CapNhatAsync(int id, CapNhatNguoiDungDto dto)
        {
            var user = await _boiCanh.Users.FindAsync(id);
            if (user == null || !user.IsActive) return false;

            user.FullName = dto.HoTen;
            user.Email = dto.Email;
            user.DienThoai = dto.DienThoai;

            return await _boiCanh.SaveChangesAsync() > 0;
        }

        public async Task<bool> XoaMemAsync(int id)
        {
            var user = await _boiCanh.Users.FindAsync(id);
            if (user == null) return false;

            user.IsActive = false;
            return await _boiCanh.SaveChangesAsync() > 0;
        }
    }
}
