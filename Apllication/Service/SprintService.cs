using Apllication.DTOs.Sprint;
using Apllication.IRepositories;
using Apllication.IService;
using Domain.Entities;
using Domain.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Apllication.Service
{
    public class SprintService : ISprintService
    {
        private readonly ISprintRepository _repository;

        public SprintService(ISprintRepository repository)
        {
            _repository = repository;
        }

        public async Task<SprintDto> GetByIdAsync(int id)
        {
            var s = await _repository.GetByIdAsync(id);
            if (s == null) return null!;

            return MapToDto(s);
        }

        public async Task<IEnumerable<SprintDto>> GetByProjectIdAsync(int projectId)
        {
            var ds = await _repository.GetByProjectIdAsync(projectId);
            return ds.Select(MapToDto);
        }

        public async Task<SprintDto> CreateAsync(TaoSprintDto dto, int creatorId)
        {
            var s = new Sprint
            {
                DuAnId = dto.DuAnId,
                TenSprint = dto.TenSprint,
                NgayBatDau = dto.NgayBatDau,
                NgayKetThuc = dto.NgayKetThuc,
                TrangThai = TrangThaiSprint.New,
                CreatedBy = creatorId
            };

            var ketQua = await _repository.AddAsync(s);
            return MapToDto(ketQua);
        }

        public async Task<bool> UpdateAsync(int id, CapNhatSprintDto dto)
        {
            var s = await _repository.GetByIdAsync(id);
            if (s == null) return false;

            s.TenSprint = dto.TenSprint;
            s.NgayBatDau = dto.NgayBatDau;
            s.NgayKetThuc = dto.NgayKetThuc;
            s.TrangThai = dto.TrangThai;

            return await _repository.UpdateAsync(s);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        private SprintDto MapToDto(Sprint s)
        {
            return new SprintDto
            {
                Id = s.Id,
                DuAnId = s.DuAnId,
                TenSprint = s.TenSprint,
                NgayBatDau = s.NgayBatDau,
                NgayKetThuc = s.NgayKetThuc,
                TrangThai = s.TrangThai,
                TienDo = (s.CongViecs != null && s.CongViecs.Count > 0)
                    ? (double)s.CongViecs.Count(c => c.TrangThai == TrangThaiCongViec.Done) / s.CongViecs.Count * 100
                    : 0
            };
        }
     }
}
