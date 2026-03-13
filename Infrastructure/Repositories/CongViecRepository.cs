using Apllication.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class CongViecRepository : ICongViecRepository
    {
        private readonly AppDbContext _context;

        public CongViecRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CongViec?> GetByIdAsync(int id)
        {
            return await _context.CongViecs
                .Include(c => c.Assignee)
                .Include(c => c.YeuCauCongViecs)
                .ThenInclude(y => y.KyNang)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<CongViec>> GetByProjectIdAsync(int projectId)
        {
            return await _context.CongViecs
                .Where(c => c.DuAnId == projectId)
                .Include(c => c.Assignee)
                .ToListAsync();
        }

        public async Task<CongViec> AddAsync(CongViec congViec)
        {
            _context.CongViecs.Add(congViec);
            await _context.SaveChangesAsync();
            return congViec;
        }

        public async Task<bool> UpdateAsync(CongViec congViec)
        {
            _context.CongViecs.Update(congViec);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
