using Apllication.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class NhatKyCongViecRepository : INhatKyCongViecRepository
    {
        private readonly AppDbContext _context;

        public NhatKyCongViecRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<NhatKyCongViec> AddAsync(NhatKyCongViec nhatKy)
        {
            _context.NhatKyCongViecs.Add(nhatKy);
            await _context.SaveChangesAsync();
            return nhatKy;
        }

        public async Task<IEnumerable<NhatKyCongViec>> GetByTaskIdAsync(int taskId)
        {
            return await _context.NhatKyCongViecs
                .Include(n => n.NguoiCapNhat)
                .Where(n => n.CongViecId == taskId)
                .OrderByDescending(n => n.NgayCapNhat)
                .ToListAsync();
        }

        public async Task<IEnumerable<NhatKyCongViec>> GetByProjectIdAsync(int projectId)
        {
            return await _context.NhatKyCongViecs
                .Include(n => n.NguoiCapNhat)
                .Include(n => n.CongViec)
                .Where(n => n.CongViec!.DuAnId == projectId)
                .OrderBy(n => n.NgayCapNhat)
                .ToListAsync();
        }
    }
}
