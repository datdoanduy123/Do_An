using Apllication.IRepositories;
using Domain.Entities;
using Domain.Enums;
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
                .Include(d => d.CongViecs)
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

        public async Task<IEnumerable<DuAnNguoiDung>> GetMembersAsync(int duAnId)
        {
            return await _context.DuAnNguoiDungs
                .Include(m => m.NguoiDung)
                    .ThenInclude(u => u.KyNangNguoiDungs)
                        .ThenInclude(k => k.KyNang)
                .Include(m => m.NguoiDung)
                    .ThenInclude(u => u.NguoiDungVaiTros)
                        .ThenInclude(uv => uv.VaiTro)
                .Include(m => m.NguoiDung)
                    .ThenInclude(u => u.CongViecs)
                .Where(m => m.DuAnId == duAnId)
                .ToListAsync();
        }

        public async Task<bool> AddMemberAsync(int duAnId, int userId, ProjectRole role = ProjectRole.Member)
        {
            var exists = await _context.DuAnNguoiDungs.AnyAsync(m => m.DuAnId == duAnId && m.NguoiDungId == userId);
            if (exists) return true;

            var membership = new DuAnNguoiDung
            {
                DuAnId = duAnId,
                NguoiDungId = userId,
                ProjectRole = role,
                JointAt = DateTime.UtcNow
            };

            _context.DuAnNguoiDungs.Add(membership);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RemoveMemberAsync(int duAnId, int userId)
        {
            var membership = await _context.DuAnNguoiDungs
                .FirstOrDefaultAsync(m => m.DuAnId == duAnId && m.NguoiDungId == userId);

            if (membership == null) return false;

            _context.DuAnNguoiDungs.Remove(membership);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateMemberRoleAsync(int duAnId, int userId, ProjectRole newRole)
        {
            var membership = await _context.DuAnNguoiDungs
                .FirstOrDefaultAsync(m => m.DuAnId == duAnId && m.NguoiDungId == userId);

            if (membership == null) return false;

            membership.ProjectRole = newRole;
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
