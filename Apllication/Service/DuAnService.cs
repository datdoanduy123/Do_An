using Apllication.DTOs.DuAn;
using Apllication.IRepositories;
using Apllication.IService;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Apllication.Service
{
    public class DuAnService : IDuAnService
    {
        private readonly IDuAnRepository _repository;
        private readonly ICongViecRepository _congViecRepo;

        public DuAnService(IDuAnRepository repository, ICongViecRepository congViecRepo)
        {
            _repository = repository;
            _congViecRepo = congViecRepo;
        }

        public async Task<DuAnDto?> GetByIdAsync(int id)
        {
            var duAn = await _repository.GetByIdAsync(id);
            if (duAn == null) return null;

            return new DuAnDto
            {
                Id = duAn.Id,
                TenDuAn = duAn.TenDuAn,
                MoTa = duAn.MoTa,
                NgayBatDau = duAn.NgayBatDau,
                NgayKetThuc = duAn.NgayKetThuc,
                TrangThai = duAn.TrangThai,
                TienDo = (duAn.CongViecs != null && duAn.CongViecs.Count > 0)
                    ? (double)duAn.CongViecs.Count(c => c.TrangThai == TrangThaiCongViec.Done) / duAn.CongViecs.Count * 100 
                    : 0,
                CreatedAt = duAn.CreatedAt
            };
        }

        public async Task<IEnumerable<DuAnDto>> GetAllAsync()
        {
            var dsDuAn = await _repository.GetAllAsync();
            return dsDuAn.Select(d => new DuAnDto
            {
                Id = d.Id,
                TenDuAn = d.TenDuAn,
                MoTa = d.MoTa,
                NgayBatDau = d.NgayBatDau,
                NgayKetThuc = d.NgayKetThuc,
                TrangThai = d.TrangThai,
                CreatedAt = d.CreatedAt
            });
        }

        public async Task<DuAnDto> CreateAsync(TaoDuAnDto dto, int creatorId)
        {
            var duAn = new DuAn
            {
                TenDuAn = dto.TenDuAn,
                MoTa = dto.MoTa,
                NgayBatDau = dto.NgayBatDau,
                NgayKetThuc = dto.NgayKetThuc,
                TrangThai = TrangThaiDuAn.Planning,
                CreatedBy = creatorId,
                CreatedAt = DateTime.UtcNow
            };

            var ketQua = await _repository.AddAsync(duAn);
            return new DuAnDto
            {
                Id = ketQua.Id,
                TenDuAn = ketQua.TenDuAn,
                MoTa = ketQua.MoTa,
                NgayBatDau = ketQua.NgayBatDau,
                NgayKetThuc = ketQua.NgayKetThuc,
                TrangThai = ketQua.TrangThai,
                CreatedAt = ketQua.CreatedAt
            };
        }

        public async Task<bool> UpdateAsync(int id, CapNhatDuAnDto dto)
        {
            var duAn = await _repository.GetByIdAsync(id);
            if (duAn == null) return false;

            duAn.TenDuAn = dto.TenDuAn;
            duAn.MoTa = dto.MoTa;
            duAn.NgayBatDau = dto.NgayBatDau;
            duAn.NgayKetThuc = dto.NgayKetThuc;
            duAn.TrangThai = dto.TrangThai;

            return await _repository.UpdateAsync(duAn);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<IEnumerable<ThanhVienDuAnDto>> GetMembersAsync(int id)
        {
            var members = await _repository.GetMembersAsync(id);
            return members.Select(m => new ThanhVienDuAnDto
            {
                Id = m.NguoiDung?.Id ?? 0,
                HoTen = m.NguoiDung?.FullName ?? "Unknown User",
                Email = m.NguoiDung?.Email ?? "No Email",
                VaiTro = m.ProjectRole,
                NgayThamGia = m.JointAt,
                SoCongViec = m.NguoiDung?.CongViecs?.Count(cv => cv.DuAnId == id) ?? 0,
                KyNang = m.NguoiDung?.KyNangNguoiDungs?
                    .Where(kn => kn.KyNang != null)
                    .Select(kn => kn.KyNang!.TenKyNang)
                    .Take(3).ToList() ?? new List<string>()
            });
        }

        public async Task<bool> AddMemberAsync(int duAnId, int userId)
        {
            return await _repository.AddMemberAsync(duAnId, userId);
        }

        public async Task<bool> RemoveMemberAsync(int id, int userId)
        {
            return await _repository.RemoveMemberAsync(id, userId);
        }

        public async Task<IEnumerable<object>> GetSkillCoverageAsync(int projectId)
        {
            // 1. Lấy danh sách task và yêu cầu kỹ năng của dự án
            var tasks = await _congViecRepo.GetTasksWithRequirementsByProjectAsync(projectId);
            
            // 2. Lấy danh sách thành viên và kỹ năng của họ trong dự án
            var members = await _repository.GetMembersAsync(projectId);
            
            // Tổng hợp các kỹ năng cần thiết
            var requiredSkills = tasks.SelectMany(t => t.YeuCauCongViecs ?? new List<YeuCauCongViec>())
                .Where(y => y.KyNang != null)
                .GroupBy(y => y.KyNang!.TenKyNang)
                .Select(g => new { Skill = g.Key, RequiredCount = g.Count() })
                .ToList();

            // Tổng hợp kỹ năng của team dự án
            var teamSkills = members
                .Where(m => m.NguoiDung != null)
                .SelectMany(m => m.NguoiDung!.KyNangNguoiDungs ?? new List<KyNangNguoiDung>())
                .Where(k => k.KyNang != null)
                .GroupBy(k => k.KyNang!.TenKyNang)
                .Select(g => new { Skill = g.Key, AvailableCount = g.Count() })
                .ToList();
 
            // Tính toán độ phủ
            var report = requiredSkills.Select(rs => {
                var available = teamSkills.FirstOrDefault(ts => ts.Skill == rs.Skill)?.AvailableCount ?? 0;
                return new {
                    Skill = rs.Skill,
                    Required = rs.RequiredCount,
                    Available = available,
                    CoveragePercent = rs.RequiredCount > 0 ? (double)available / rs.RequiredCount * 100 : 100
                };
            }).ToList();

            return report;
        }
    }
}
