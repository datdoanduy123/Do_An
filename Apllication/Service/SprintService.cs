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
        // Inject repository quy tắc AI để đọc DEFAULT_SPRINT_DAYS thay vì hard-code 14 ngày
        private readonly IQuyTacGiaoViecAIRepository _ruleRepo;

        public SprintService(ISprintRepository repository, IQuyTacGiaoViecAIRepository ruleRepo)
        {
            _repository = repository;
            _ruleRepo = ruleRepo;
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
            var success = await _repository.UpdateAsync(s);

            // Nếu Sprint vừa được đánh dấu là Finished (2), tự động kích hoạt Sprint kế tiếp
            if (success && s.TrangThai == TrangThaiSprint.Finished)
            {
                await TuDongMoSprintTiepTheoAsync(id);
            }

            return success;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        /// <summary>
        /// Kích hoạt Sprint: Người dùng bấm nút [▶ Kích hoạt] trên giao diện.
        /// Logic mới: Sprint phải đang ở New mới kích hoạt được → Chuyển sang InProgress.
        /// Ràng buộc sprint tuần tự: Nếu dự án đã có 1 Sprint đang InProgress thì CHẶN, trả lỗi.
        /// Số ngày Sprint đọc từ bảng QuyTacGiaoViecAI (mã DEFAULT_SPRINT_DAYS, mặc định 14).
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

            // Kiểm tra ràng buộc tuần tự: Trong dự án này không được có sprint nào đang InProgress
            var allSprints = await _repository.GetByProjectIdAsync(sprint.DuAnId);
            bool hasActiveSprint = allSprints.Any(s => s.Id != sprintId && s.TrangThai == TrangThaiSprint.InProgress);
            if (hasActiveSprint)
                throw new InvalidOperationException(
                    "Không thể kích hoạt! Dự án đang có một Sprint đang chạy. " +
                    "Sprint hiện tại phải kết thúc trước khi Sprint mới được bắt đầu."
                );

            // Đọc số ngày sprint từ bảng QuyTacGiaoViecAI
            int sprintDays = await LaySoNgaySprintAsync();

            // Cập nhật trạng thái và thiết lập mốc thời gian dựa trên cấu hình 
            sprint.TrangThai = TrangThaiSprint.InProgress;
            sprint.NgayBatDau = DateTime.UtcNow.Date;
            sprint.NgayKetThuc = DateTime.UtcNow.Date.AddDays(sprintDays);

            await _repository.UpdateAsync(sprint);

            return MapToDto(sprint);
        }

        /// <summary>
        /// Tự động mở Sprint tiếp theo trong dự án khi Sprint hiện tại vừa được đặt là Finished.
        /// Quy tắc tìm Sprint kế:
        ///   - Cùng dự án (DuAnId)
        ///   - Trạng thái = New (chưa chạy)
        ///   - Ưu tiên: NgayBatDau nhỏ nhất → nếu bằng nhau thì Id nhỏ nhất (sprint được tạo trước)
        /// Số ngày chạy cũng đọc từ QuyTacGiaoViecAI.
        /// </summary>
        public async Task TuDongMoSprintTiepTheoAsync(int completedSprintId)
        {
            // Lấy thông tin sprint vừa hoàn thành để biết DuAnId
            var completedSprint = await _repository.GetByIdAsync(completedSprintId);
            if (completedSprint == null) return;

            // Lấy tất cả sprint của dự án để tìm sprint kế tiếp
            var allSprints = await _repository.GetByProjectIdAsync(completedSprint.DuAnId);

            // Tìm sprint New có NgayBatDau nhỏ nhất (tiếp theo trong chuỗi tuần tự)
            var nextSprint = allSprints
                .Where(s => s.TrangThai == TrangThaiSprint.New)
                .OrderBy(s => s.NgayBatDau)
                .ThenBy(s => s.Id)
                .FirstOrDefault();

            if (nextSprint == null) return; // Không còn sprint nào tiếp theo → dự án đã hoàn thành tất cả sprint

            // Đọc số ngày từ cấu hình
            int sprintDays = await LaySoNgaySprintAsync();

            // Tự động kích hoạt sprint tiếp theo ngay lập tức
            nextSprint.TrangThai = TrangThaiSprint.InProgress;
            nextSprint.NgayBatDau = DateTime.UtcNow.Date;
            nextSprint.NgayKetThuc = DateTime.UtcNow.Date.AddDays(sprintDays);

            await _repository.UpdateAsync(nextSprint);
        }

        /// <summary>
        /// Đọc số ngày mặc định của một Sprint từ bảng QuyTacGiaoViecAI.
        /// Mã quy tắc: DEFAULT_SPRINT_DAYS — mặc định 14 ngày nếu chưa cấu hình.
        /// </summary>
        private async Task<int> LaySoNgaySprintAsync()
        {
            var rules = await _ruleRepo.GetAllActiveRulesAsync();
            var rule = rules.FirstOrDefault(r => r.MaQuyTac == "DEFAULT_SPRINT_DAYS");
            if (rule != null && double.TryParse(rule.GiaTri, out double val) && val > 0)
                return (int)val;
            return 14; // Mặc định 14 ngày = 2 tuần
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
