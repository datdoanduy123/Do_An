using Apllication.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class QuyTacGiaoViecAIRepository : IQuyTacGiaoViecAIRepository
    {
        private readonly AppDbContext _context;

        public QuyTacGiaoViecAIRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<QuyTacGiaoViecAI>> GetAllAsync()
        {
            return await _context.QuyTacGiaoViecAIs.ToListAsync();
        }

        public async Task<QuyTacGiaoViecAI?> GetByIdAsync(int id)
        {
            return await _context.QuyTacGiaoViecAIs.FindAsync(id);
        }

        public async Task<bool> UpdateAsync(QuyTacGiaoViecAI rule)
        {
            _context.QuyTacGiaoViecAIs.Update(rule);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<QuyTacGiaoViecAI>> GetAllActiveRulesAsync()
        {
            return await _context.QuyTacGiaoViecAIs
                .Where(r => r.IsActive)
                .ToListAsync();
        }

        public async Task<QuyTacGiaoViecAI?> GetByCodeAsync(string code)
        {
            return await _context.QuyTacGiaoViecAIs
                .FirstOrDefaultAsync(r => r.MaQuyTac == code && r.IsActive);
        }
    }
}
