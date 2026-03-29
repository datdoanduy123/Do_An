using Apllication.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class TraoLoiRepository : ITraoLoiRepository
    {
        private readonly AppDbContext _context;

        public TraoLoiRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<TraoLoiCongViec> GetByIdAsync(int id)
        {
            return await _context.TraoLoiCongViecs
                .Include(x => x.NguoiTao)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<TraoLoiCongViec>> GetByCongViecIdAsync(int congViecId)
        {
            return await _context.TraoLoiCongViecs
                .Include(x => x.NguoiTao)
                .Where(x => x.CongViecId == congViecId)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<TraoLoiCongViec> AddAsync(TraoLoiCongViec traoLoi)
        {
            await _context.TraoLoiCongViecs.AddAsync(traoLoi);
            await _context.SaveChangesAsync();
            return traoLoi;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var traoLoi = await _context.TraoLoiCongViecs.FindAsync(id);
            if (traoLoi == null) return false;

            _context.TraoLoiCongViecs.Remove(traoLoi);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
