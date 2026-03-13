using Apllication.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class TaiLieuDuAnRepository : ITaiLieuDuAnRepository
    {
        private readonly AppDbContext _context;

        public TaiLieuDuAnRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<TaiLieuDuAn?> GetByIdAsync(int id)
        {
            return await _context.TaiLieuDuAns.FindAsync(id);
        }

        public async Task<IEnumerable<TaiLieuDuAn>> GetByProjectIdAsync(int projectId)
        {
            return await _context.TaiLieuDuAns
                .Where(t => t.DuAnId == projectId)
                .ToListAsync();
        }

        public async Task<TaiLieuDuAn> AddAsync(TaiLieuDuAn taiLieu)
        {
            _context.TaiLieuDuAns.Add(taiLieu);
            await _context.SaveChangesAsync();
            return taiLieu;
        }

        public async Task<bool> UpdateAsync(TaiLieuDuAn taiLieu)
        {
            _context.TaiLieuDuAns.Update(taiLieu);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var taiLieu = await _context.TaiLieuDuAns.FindAsync(id);
            if (taiLieu == null) return false;
            _context.TaiLieuDuAns.Remove(taiLieu);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
