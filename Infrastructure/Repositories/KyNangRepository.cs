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
    }
}
