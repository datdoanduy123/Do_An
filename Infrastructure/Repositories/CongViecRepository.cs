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
                .Include(c => c.Sprint)
                .Include(c => c.Assignee)
                .Include(c => c.YeuCauCongViecs)
                .ThenInclude(y => y.KyNang)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<CongViec>> GetByProjectIdAsync(int projectId)
        {
            return await _context.CongViecs
                .Where(c => c.DuAnId == projectId)
                .Include(c => c.Sprint)
                .Include(c => c.Assignee)
                .ToListAsync();
        }

        public async Task<IEnumerable<CongViec>> GetByAssigneeIdAsync(int assigneeId)
        {
            return await _context.CongViecs
                .Where(c => c.AssigneeId == assigneeId)
                .Include(c => c.Sprint)
                .Include(c => c.Assignee)
                .ToListAsync();
        }

        public async Task<IEnumerable<CongViec>> GetAllAsync()
        {
            return await _context.CongViecs
                .Include(c => c.Sprint)
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

        public async Task<bool> DeleteAsync(int id)
        {
            var cv = await _context.CongViecs.FindAsync(id);
            if (cv == null) return false;
            _context.CongViecs.Remove(cv);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Apllication.DTOs.PagedResultDto<CongViec>> LayDanhSachCongViecAsync(Apllication.DTOs.CongViecQueryDto query)
        {
            // Fix #1: Load sẵn YeuCauCongViecs và Dependencies để tránh N+1 query
            // khi GiaoViecAIService duyệt từng task để tính điểm kỹ năng và xử lý dependency
            var dbQuery = _context.CongViecs
                .Include(c => c.Sprint)
                .Include(c => c.Assignee)
                .Include(c => c.YeuCauCongViecs)
                    .ThenInclude(y => y.KyNang)
                .Include(c => c.Dependencies)
                .AsQueryable();

            if (query.DuAnId.HasValue)
            {
                dbQuery = dbQuery.Where(c => c.DuAnId == query.DuAnId.Value);
            }

            if (query.AssigneeId.HasValue)
            {
                dbQuery = dbQuery.Where(c => c.AssigneeId == query.AssigneeId.Value);
            }

            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                dbQuery = dbQuery.Where(c => c.TieuDe.Contains(query.SearchTerm) || c.MoTa!.Contains(query.SearchTerm));
            }

            //if (query.TrangThai.HasValue)
            //{
            //    dbQuery = dbQuery.Where(c => c.TrangThai == query.TrangThai.Value);
            //}

            var totalCount = await dbQuery.CountAsync();
            var items = await dbQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new Apllication.DTOs.PagedResultDto<CongViec>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        public async Task<IEnumerable<CongViec>> GetTasksWithRequirementsByProjectAsync(int projectId)
        {
            return await _context.CongViecs
                .Where(c => c.DuAnId == projectId)
                .Include(c => c.YeuCauCongViecs)
                .ThenInclude(y => y.KyNang)
                .ToListAsync();
        }
    }
}
