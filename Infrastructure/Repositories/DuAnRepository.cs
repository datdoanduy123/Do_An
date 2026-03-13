using Apllication.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class DuAnRepository : IDuAnRepository
    {
        private readonly AppDbContext _context;

        public DuAnRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DuAn?> GetByIdAsync(int id)
        {
            return await _context.DuAns
                .Include(d => d.TaiLieuDuAns)
                .Include(d => d.Sprints)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<IEnumerable<DuAn>> GetAllAsync()
        {
            return await _context.DuAns.ToListAsync();
        }

        public async Task<DuAn> AddAsync(DuAn duAn)
        {
            _context.DuAns.Add(duAn);
            await _context.SaveChangesAsync();
            return duAn;
        }

        public async Task<bool> UpdateAsync(DuAn duAn)
        {
            _context.DuAns.Update(duAn);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var duAn = await _context.DuAns.FindAsync(id);
            if (duAn == null) return false;
            _context.DuAns.Remove(duAn);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
