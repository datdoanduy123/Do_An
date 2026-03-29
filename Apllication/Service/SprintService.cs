using Apllication.DTOs.Sprint;
using Apllication.IRepositories;
using Apllication.IService;
using Domain.Entities;
using Domain.Enums;
using System;                    // Cần thiết cho DateTime.UtcNow
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

        /// <summary>
        /// Kích hoạt Sprint: Người dùng bấm nút [▶ Kích hoạt] trên giao diện.
        /// Logic: Sprint phải đang ở New mới kích hoạt được → Chuyển sang InProgress.
        /// Tự động set ngày bắt đầu = hôm nay, ngày kết thúc = hôm nay + 14 ngày (2 tuần).
        /// Cho phép nhiều Sprint kích hoạt song song (không giới hạn) để hỗ trợ nhiều team.
        /// </summary>
        public async Task<SprintDto?> KichHoatSprintAsync(int sprintId, int userId)
        {
            var sprint = await _repository.GetByIdAsync(sprintId);
            if (sprint == null) return null;

            // Chỉ Sprint đang ở trạng thái New mới được phép kích hoạt
            if (sprint.TrangThai != TrangThaiSprint.New)
                throw new InvalidOperationException(
                    sprint.TrangThai == TrangThaiSprint.InProgress
                        ? "Sprint này đã được kích hoạt và đang chạy."
                        : "Sprint đã kết thúc, không thể kích hoạt lại."
                );

            // Cập nhật trạng thái và thiết lập mốc thời gian chính xác
            sprint.TrangThai = TrangThaiSprint.InProgress;
            sprint.NgayBatDau = DateTime.UtcNow.Date;           // Bắt đầu ngay hôm nay
            sprint.NgayKetThuc = DateTime.UtcNow.Date.AddDays(14); // Kết thúc sau đúng 2 tuần

            await _repository.UpdateAsync(sprint);

            return MapToDto(sprint);
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
