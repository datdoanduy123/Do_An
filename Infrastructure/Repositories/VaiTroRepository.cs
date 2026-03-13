using Apllication.DTOs;
using Apllication.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Helpers;

namespace Infrastructure.Repositories
{
    // Trien khai Repository cho thuc the VaiTro
    public class VaiTroRepository : IVaiTroRepository
    {
        private readonly AppDbContext _boiCanh;

        public VaiTroRepository(AppDbContext boiCanh)
        {
            _boiCanh = boiCanh;
        }

        public async Task<bool> KiemTraTenVaiTroTonTaiAsync(string tenVaiTro)
        {
            return await _boiCanh.VaiTros.AnyAsync(x => x.TenVaiTro == tenVaiTro);
        }

        public async Task<VaiTroDto> TaoVaiTroAsync(TaoVaiTroDto taoVaiTroDto)
        {
            var vaiTro = new VaiTro
            {
                TenVaiTro = taoVaiTroDto.TenVaiTro,
                MaVaiTro = taoVaiTroDto.MaVaiTro,
                MoTa = taoVaiTroDto.MoTa,
                CreatedAt = DateTime.UtcNow
            };

            await _boiCanh.VaiTros.AddAsync(vaiTro);
            await _boiCanh.SaveChangesAsync();

            return new VaiTroDto
            {
                Id = vaiTro.Id,
                TenVaiTro = vaiTro.TenVaiTro,
                MaVaiTro = vaiTro.MaVaiTro,
                MoTa = vaiTro.MoTa
            };
        }

        public async Task<bool> GanVaiTroChoNguoiDungAsync(GanVaiTroDto ganVaiTroDto)
        {
            // Kiem tra xem da ton tai chua de tranh trung lap
            var tonTai = await _boiCanh.NguoiDungVaiTros.AnyAsync(x => 
                x.NguoiDungId == ganVaiTroDto.NguoiDungId && x.VaiTroId == ganVaiTroDto.VaiTroId);
            
            if (tonTai) return true;

            var link = new NguoiDungVaiTro
            {
                NguoiDungId = ganVaiTroDto.NguoiDungId,
                VaiTroId = ganVaiTroDto.VaiTroId
            };

            await _boiCanh.NguoiDungVaiTros.AddAsync(link);
            return await _boiCanh.SaveChangesAsync() > 0;
        }

        public async Task<bool> GanQuyenChoVaiTroAsync(GanQuyenChoVaiTroDto ganQuyenDto)
        {
            // Kiem tra xem ban ghi da ton tai chua
            var tonTai = await _boiCanh.VaiTroQuyens.AnyAsync(x => 
                x.VaiTroId == ganQuyenDto.VaiTroId && x.QuyenId == ganQuyenDto.QuyenId);
            
            if (tonTai) return true;

            var link = new VaiTroQuyen
            {
                VaiTroId = ganQuyenDto.VaiTroId,
                QuyenId = ganQuyenDto.QuyenId
            };

            await _boiCanh.VaiTroQuyens.AddAsync(link);
            return await _boiCanh.SaveChangesAsync() > 0;
        }

        public async Task<KetQuaPhanTrangDto<VaiTroDto>> LayDanhSachVaiTroAsync(VaiTroQueryDto query)
        {
            var queryable = _boiCanh.VaiTros.AsQueryable();

            if (!string.IsNullOrEmpty(query.Keyword))
            {
                var k = query.Keyword.ToLower();
                queryable = queryable.Where(x => x.TenVaiTro.ToLower().Contains(k) || x.MaVaiTro.ToLower().Contains(k));
            }

            return await queryable
                .Select(x => new VaiTroDto
                {
                    Id = x.Id,
                    TenVaiTro = x.TenVaiTro,
                    MaVaiTro = x.MaVaiTro,
                    MoTa = x.MoTa
                })
                .ToPagedListAsync(query.PageIndex, query.PageSize);
        }

        public async Task<VaiTroDto?> LayTheoIdAsync(int id)
        {
            var vaiTro = await _boiCanh.VaiTros.FindAsync(id);
            if (vaiTro == null) return null;

            return new VaiTroDto
            {
                Id = vaiTro.Id,
                TenVaiTro = vaiTro.TenVaiTro,
                MaVaiTro = vaiTro.MaVaiTro,
                MoTa = vaiTro.MoTa
            };
        }

        public async Task<bool> CapNhatAsync(int id, CapNhatVaiTroDto dto)
        {
            var vaiTro = await _boiCanh.VaiTros.FindAsync(id);
            if (vaiTro == null) return false;

            vaiTro.TenVaiTro = dto.TenVaiTro;
            vaiTro.MoTa = dto.MoTa;

            return await _boiCanh.SaveChangesAsync() > 0;
        }

        public async Task<bool> XoaAsync(int id)
        {
            var vaiTro = await _boiCanh.VaiTros.FindAsync(id);
            if (vaiTro == null) return false;

            _boiCanh.VaiTros.Remove(vaiTro);
            return await _boiCanh.SaveChangesAsync() > 0;
        }

        public async Task<bool> GoVaiTroKhoiNguoiDungAsync(GanVaiTroDto dto)
        {
            var banGhi = await _boiCanh.NguoiDungVaiTros.FirstOrDefaultAsync(x => 
                x.NguoiDungId == dto.NguoiDungId && x.VaiTroId == dto.VaiTroId);
            
            if (banGhi == null) return true;

            _boiCanh.NguoiDungVaiTros.Remove(banGhi);
            return await _boiCanh.SaveChangesAsync() > 0;
        }

        public async Task<bool> GoQuyenKhoiVaiTroAsync(GanQuyenChoVaiTroDto dto)
        {
            var banGhi = await _boiCanh.VaiTroQuyens.FirstOrDefaultAsync(x => 
                x.VaiTroId == dto.VaiTroId && x.QuyenId == dto.QuyenId);

            if (banGhi == null) return true;

            _boiCanh.VaiTroQuyens.Remove(banGhi);
            return await _boiCanh.SaveChangesAsync() > 0;
        }

        public async Task<List<QuyenDto>> LayDanhSachQuyenTheoVaiTroAsync(int vaiTroId)
        {
            return await _boiCanh.VaiTroQuyens
                .Where(x => x.VaiTroId == vaiTroId)
                .Join(_boiCanh.Quyens, 
                    vq => vq.QuyenId, 
                    q => q.Id, 
                    (vq, q) => new QuyenDto
                    {
                        Id = q.Id,
                        TenQuyen = q.TenQuyen,
                        MaQuyen = q.MaQuyen,
                        MoTa = q.MoTa,
                        NhomQuyenId = q.NhomQuyenId
                    })
                .ToListAsync();
        }
    }
}
