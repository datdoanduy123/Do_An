using Apllication.DTOs;
using Apllication.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Helpers;

namespace Infrastructure.Repositories
{
    // Trien khai Repository cho thuc the Quyen
    public class QuyenRepository : IQuyenRepository
    {
        private readonly AppDbContext _boiCanh;

        public QuyenRepository(AppDbContext boiCanh)
        {
            _boiCanh = boiCanh;
        }

        public async Task<bool> KiemTraMaQuyenTonTaiAsync(string maQuyen)
        {
            return await _boiCanh.Quyens.AnyAsync(x => x.MaQuyen == maQuyen);
        }

        public async Task<QuyenDto> TaoQuyenAsync(TaoQuyenDto taoQuyenDto)
        {
            var quyen = new Quyen
            {
                TenQuyen = taoQuyenDto.TenQuyen,
                MaQuyen = taoQuyenDto.MaQuyen,
                MoTa = taoQuyenDto.MoTa,
                NhomQuyenId = taoQuyenDto.NhomQuyenId,
                CreatedAt = DateTime.UtcNow
            };

            await _boiCanh.Quyens.AddAsync(quyen);
            await _boiCanh.SaveChangesAsync();

            // Map sang DTO de tra ve (Dung de hien thi thong tin vua tao)
            return new QuyenDto
            {
                Id = quyen.Id,
                TenQuyen = quyen.TenQuyen,
                MaQuyen = quyen.MaQuyen,
                MoTa = quyen.MoTa,
                NhomQuyenId = quyen.NhomQuyenId
            };
        }

        public async Task<KetQuaPhanTrangDto<QuyenDto>> LayDanhSachQuyenAsync(QuyenQueryDto query)
        {
            var queryable = _boiCanh.Quyens.AsQueryable();

            if (!string.IsNullOrEmpty(query.Keyword))
            {
                var k = query.Keyword.ToLower();
                queryable = queryable.Where(x => x.TenQuyen.ToLower().Contains(k) || x.MaQuyen.ToLower().Contains(k));
            }

            return await queryable
                .Select(x => new QuyenDto
                {
                    Id = x.Id,
                    TenQuyen = x.TenQuyen,
                    MaQuyen = x.MaQuyen,
                    MoTa = x.MoTa,
                    NhomQuyenId = x.NhomQuyenId
                })
                .ToPagedListAsync(query.PageIndex, query.PageSize);
        }

        public async Task<QuyenDto?> LayTheoIdAsync(int id)
        {
            var quyen = await _boiCanh.Quyens.FindAsync(id);
            if (quyen == null) return null;

            return new QuyenDto
            {
                Id = quyen.Id,
                TenQuyen = quyen.TenQuyen,
                MaQuyen = quyen.MaQuyen,
                MoTa = quyen.MoTa,
                NhomQuyenId = quyen.NhomQuyenId
            };
        }

        public async Task<bool> CapNhatAsync(int id, CapNhatQuyenDto dto)
        {
            var quyen = await _boiCanh.Quyens.FindAsync(id);
            if (quyen == null) return false;

            quyen.TenQuyen = dto.TenQuyen;
            quyen.MoTa = dto.MoTa;
            quyen.NhomQuyenId = dto.NhomQuyenId;

            return await _boiCanh.SaveChangesAsync() > 0;
        }

        public async Task<bool> XoaAsync(int id)
        {
            var quyen = await _boiCanh.Quyens.FindAsync(id);
            if (quyen == null) return false;

            _boiCanh.Quyens.Remove(quyen);
            return await _boiCanh.SaveChangesAsync() > 0;
        }
    }
}
