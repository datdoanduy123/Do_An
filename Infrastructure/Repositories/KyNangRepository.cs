using Apllication.DTOs;
using Apllication.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class KyNangRepository : IKyNangRepository
    {
        private readonly AppDbContext _context;

        public KyNangRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<KyNang>> GetAllAsync()
        {
            return await _context.KyNangs.ToListAsync();
        }

        public async Task<KyNang?> GetByNameAsync(string name)
        {
            return await _context.KyNangs
                .FirstOrDefaultAsync(k => k.TenKyNang.ToLower() == name.ToLower());
        }

        public async Task<KetQuaPhanTrangDto<KyNangDto>> LayDanhSachKyNangAsync(KyNangQueryDto query)
        {
            var queryable = _context.KyNangs
                .Include(k => k.CongNghe)
                .ThenInclude(cn => cn.NhomKyNang)
                .AsQueryable();

            if (!string.IsNullOrEmpty(query.Keyword))
            {
                queryable = queryable.Where(k => k.TenKyNang.Contains(query.Keyword));
            }

            var totalCount = await queryable.CountAsync();
            var items = await queryable
                .OrderBy(k => k.TenKyNang)
                .Skip((query.PageIndex - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(k => new KyNangDto
                {
                    Id = k.Id,
                    TenKyNang = k.TenKyNang,
                    MoTa = k.MoTa,
                    CongNgheId = k.CongNgheId,
                    TenCongNghe = k.CongNghe.TenCongNghe,
                    TenNhomKyNang = k.CongNghe.NhomKyNang.TenNhom
                })
                .ToListAsync();

            return new KetQuaPhanTrangDto<KyNangDto>(items, totalCount, query.PageIndex, query.PageSize);
        }

        public async Task<KyNang?> GetByIdAsync(int id)
        {
            return await _context.KyNangs
                .Include(k => k.CongNghe)
                .ThenInclude(cn => cn.NhomKyNang)
                .FirstOrDefaultAsync(k => k.Id == id);
        }

        public async Task AddAsync(KyNang kyNang)
        {
            await _context.KyNangs.AddAsync(kyNang);
        }

        public async Task UpdateAsync(KyNang kyNang)
        {
            _context.KyNangs.Update(kyNang);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(KyNang kyNang)
        {
            _context.KyNangs.Remove(kyNang);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<NhomKyNang>> GetAllNhomKyNangAsync()
        {
            return await _context.NhomKyNangs.OrderBy(n => n.TenNhom).ToListAsync();
        }

        public async Task<IEnumerable<CongNghe>> GetCongNgheByNhomAsync(int nhomId)
        {
            return await _context.CongNghes
                .Where(c => c.NhomKyNangId == nhomId)
                .OrderBy(c => c.TenCongNghe)
                .ToListAsync();
        }

        public async Task<IEnumerable<NhomKyNang>> GetHierarchyAsync()
        {
            return await _context.NhomKyNangs
                .Include(n => n.CongNghes)
                .ThenInclude(c => c.KyNangs)
                .OrderBy(n => n.TenNhom)
                .ToListAsync();
        }

        public async Task AddNhomAsync(NhomKyNang nhom)
        {
            await _context.NhomKyNangs.AddAsync(nhom);
        }

        public async Task AddCongNgheAsync(CongNghe cn)
        {
            await _context.CongNghes.AddAsync(cn);
        }

        public async Task<NhomKyNang?> GetNhomByIdAsync(int id)
        {
            return await _context.NhomKyNangs.FindAsync(id);
        }

        public async Task<CongNghe?> GetCongNgheByIdAsync(int id)
        {
            return await _context.CongNghes.FindAsync(id);
        }
    }
}
