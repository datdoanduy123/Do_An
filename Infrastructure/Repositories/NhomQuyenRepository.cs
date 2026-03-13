using Apllication.DTOs;
using Apllication.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Helpers;

namespace Infrastructure.Repositories
{
    // Trien khai Repository cho thuc the NhomQuyen
    public class NhomQuyenRepository : INhomQuyenRepository
    {
        private readonly AppDbContext _boiCanh;

        public NhomQuyenRepository(AppDbContext boiCanh)
        {
            _boiCanh = boiCanh;
        }

        public async Task<bool> KiemTraTenNhomTonTaiAsync(string tenNhom)
        {
            return await _boiCanh.NhomQuyens.AnyAsync(x => x.TenNhom == tenNhom);
        }

        public async Task<NhomQuyenDto> TaoNhomQuyenAsync(TaoNhomQuyenDto taoNhomQuyenDto)
        {
            var nhomQuyen = new NhomQuyen
            {
                TenNhom = taoNhomQuyenDto.TenNhom,
                MoTa = taoNhomQuyenDto.MoTa
            };

            await _boiCanh.NhomQuyens.AddAsync(nhomQuyen);
            await _boiCanh.SaveChangesAsync();

            return new NhomQuyenDto
            {
                Id = nhomQuyen.Id,
                TenNhom = nhomQuyen.TenNhom,
                MoTa = nhomQuyen.MoTa
            };
        }

        public async Task<KetQuaPhanTrangDto<NhomQuyenDto>> LayDanhSachNhomQuyenAsync(NhomQuyenQueryDto query)
        {
            var queryable = _boiCanh.NhomQuyens.AsQueryable();

            if (!string.IsNullOrEmpty(query.Keyword))
            {
                var k = query.Keyword.ToLower();
                queryable = queryable.Where(x => x.TenNhom.ToLower().Contains(k));
            }

            return await queryable
                .Select(x => new NhomQuyenDto
                {
                    Id = x.Id,
                    TenNhom = x.TenNhom,
                    MoTa = x.MoTa
                })
                .ToPagedListAsync(query.PageIndex, query.PageSize);
        }

        public async Task<NhomQuyenDto?> LayTheoIdAsync(int id)
        {
            var nhom = await _boiCanh.NhomQuyens.FindAsync(id);
            if (nhom == null) return null;

            return new NhomQuyenDto
            {
                Id = nhom.Id,
                TenNhom = nhom.TenNhom,
                MoTa = nhom.MoTa
            };
        }

        public async Task<bool> CapNhatAsync(int id, CapNhatNhomQuyenDto dto)
        {
            var nhom = await _boiCanh.NhomQuyens.FindAsync(id);
            if (nhom == null) return false;

            nhom.TenNhom = dto.TenNhom;
            nhom.MoTa = dto.MoTa;

            return await _boiCanh.SaveChangesAsync() > 0;
        }

        public async Task<bool> XoaAsync(int id)
        {
            var nhom = await _boiCanh.NhomQuyens.FindAsync(id);
            if (nhom == null) return false;

            _boiCanh.NhomQuyens.Remove(nhom);
            return await _boiCanh.SaveChangesAsync() > 0;
        }
    }
}
