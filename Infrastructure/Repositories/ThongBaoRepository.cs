using Domain.Entities;
using Apllication.IRepositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class ThongBaoRepository : IThongBaoRepository
    {
        private readonly AppDbContext _context;

        public ThongBaoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddAsync(ThongBao thongBao)
        {
            await _context.ThongBaos.AddAsync(thongBao);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAllAsync(int userId)
        {
            var notifs = await _context.ThongBaos.Where(x => x.UserId == userId).ToListAsync();
            _context.ThongBaos.RemoveRange(notifs);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<ThongBao>> GetByUserIdAsync(int userId, int limit = 20)
        {
            return await _context.ThongBaos
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _context.ThongBaos.CountAsync(x => x.UserId == userId && !x.IsRead);
        }

        public async Task<bool> MarkAllAsReadAsync(int userId)
        {
            var unread = await _context.ThongBaos.Where(x => x.UserId == userId && !x.IsRead).ToListAsync();
            foreach (var item in unread)
            {
                item.IsRead = true;
            }
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> MarkAsReadAsync(int notificationId)
        {
            var notif = await _context.ThongBaos.FindAsync(notificationId);
            if (notif == null) return false;
            
            notif.IsRead = true;
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
