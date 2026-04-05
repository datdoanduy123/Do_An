using Apllication.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class SprintRepository : ISprintRepository
    {
        private readonly AppDbContext _context;

        public SprintRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Sprint?> GetByIdAsync(int id)
        {
            return await _context.Sprints
                .Include(s => s.CongViecs)
                    .ThenInclude(c => c.TraoLoiCongViecs)
                        .ThenInclude(tl => tl.NguoiTao)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<Sprint>> GetByProjectIdAsync(int projectId)
        {
            return await _context.Sprints
                .Where(s => s.DuAnId == projectId)
                .Include(s => s.CongViecs)
                    .ThenInclude(c => c.TraoLoiCongViecs)
                        .ThenInclude(tl => tl.NguoiTao)
                .ToListAsync();
        }
 
        public async Task<Sprint> AddAsync(Sprint sprint)
        {
            _context.Sprints.Add(sprint);
            await _context.SaveChangesAsync();
            return sprint;
        }

        public async Task<bool> UpdateAsync(Sprint sprint)
        {
            _context.Sprints.Update(sprint);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var sprint = await _context.Sprints.FindAsync(id);
            if (sprint == null) return false;
            _context.Sprints.Remove(sprint);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
