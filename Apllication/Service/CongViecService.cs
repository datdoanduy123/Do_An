using Apllication.DTOs.CongViec;
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
    public class CongViecService : ICongViecService
    {
        private readonly ICongViecRepository _repository;
        private readonly INguoiDungRepository _userRepository;
        private readonly INhatKyCongViecRepository _taskLogRepository;
        private readonly ISprintRepository _sprintRepository;
        private readonly IKanbanNotificationService _notificationService;

        public CongViecService(
            ICongViecRepository repository, 
            INguoiDungRepository userRepository,
            INhatKyCongViecRepository taskLogRepository,
            ISprintRepository sprintRepository,
            IKanbanNotificationService notificationService)
        {
            _repository = repository;
            _userRepository = userRepository;
            _taskLogRepository = taskLogRepository;
            _sprintRepository = sprintRepository;
            _notificationService = notificationService;
        }

        public async Task<CongViecDto> GetByIdAsync(int id)
        {
            var cv = await _repository.GetByIdAsync(id);
            if (cv == null) return null!;

            return MapToDto(cv);
        }

        public async Task<IEnumerable<CongViecDto>> GetByProjectIdAsync(int projectId)
        {
            var dsCv = await _repository.GetByProjectIdAsync(projectId);
            return dsCv.Select(MapToDto);
        }

        public async Task<IEnumerable<CongViecDto>> GetMyTasksAsync(int userId)
        {
            var dsCv = await _repository.GetByAssigneeIdAsync(userId);
            return dsCv.Select(MapToDto);
        }

        public async Task<CongViecDto> CreateAsync(TaoCongViecDto dto, int creatorId)
        {
            var cv = new CongViec
            {
                DuAnId = dto.DuAnId,
                SprintId = dto.SprintId,
                TieuDe = dto.TieuDe,
                MoTa = dto.MoTa,
                LoaiCongViec = dto.LoaiCongViec,
                DoUuTien = dto.DoUuTien,
                TrangThai = TrangThaiCongViec.Todo,
                AssigneeId = dto.AssigneeId,
                ThoiGianUocTinh = dto.ThoiGianUocTinh,
                NgayBatDauDuKien = dto.NgayBatDauDuKien,
                NgayKetThucDuKien = dto.NgayKetThucDuKien,
                PhuongThucGiaoViec = PhuongThucGiaoViec.Manual,
                CreatedBy = creatorId,
                CreatedAt = DateTime.UtcNow
            };

            var ketQua = await _repository.AddAsync(cv);
            
            // Thông báo realtime cho dự án (Bảng Kanban)
            await _notificationService.NotifyTaskUpdated(cv.DuAnId);

            // Nếu gán việc ngay khi tạo, gửi thông báo cá nhân cho người thực hiện
            if (cv.AssigneeId.HasValue && cv.AssigneeId != creatorId)
            {
                await _notificationService.NotifyPersonal(
                    cv.AssigneeId.Value, 
                    "Công việc mới", 
                    $"Bạn vừa được gán công việc mới: '{cv.TieuDe}' trong dự án."
                );
            }

            return MapToDto(cv);
        }

        public async Task<CongViecDto> UpdateAsync(int id, CapNhatCongViecDto dto)
        {
            var cv = await _repository.GetByIdAsync(id);
            if (cv == null) throw new Exception("Không tìm thấy công việc.");

            int? oldAssigneeId = cv.AssigneeId;

            cv.TieuDe = dto.TieuDe;
            cv.MoTa = dto.MoTa;
            cv.LoaiCongViec = dto.LoaiCongViec;
            cv.DoUuTien = dto.DoUuTien;
            cv.AssigneeId = dto.AssigneeId;
            cv.ThoiGianUocTinh = dto.ThoiGianUocTinh;

            await _repository.UpdateAsync(cv);

            // Thông báo realtime bảng Kanban
            await _notificationService.NotifyTaskUpdated(cv.DuAnId);

            // Nếu có thay đổi người thực hiện: RESET bộ đếm phạt, điều chỉnh KhoiLuongCongViec và gửi thông báo
            if (cv.AssigneeId != oldAssigneeId)
            {
                cv.SoLanBiTuChoi = 0; // Reset phạt khi đổi người

                // Trừ workload người cũ (User A không còn làm task này nữa)
                if (oldAssigneeId.HasValue)
                {
                    var oldUser = await _userRepository.LayTheoIdAsync(oldAssigneeId.Value);
                    if (oldUser != null)
                    {
                        oldUser.KhoiLuongCongViec = Math.Max(0, oldUser.KhoiLuongCongViec - cv.ThoiGianUocTinh);
                        await _userRepository.UpdateAsync(oldUser);
                    }
                }

                // Cộng workload người mới (User B nhận thêm task này)
                if (cv.AssigneeId.HasValue)
                {
                    var newUser = await _userRepository.LayTheoIdAsync(cv.AssigneeId.Value);
                    if (newUser != null)
                    {
                        newUser.KhoiLuongCongViec += cv.ThoiGianUocTinh;
                        await _userRepository.UpdateAsync(newUser);
                    }

                    // Thông báo cho người mới
                    await _notificationService.NotifyPersonal(
                        cv.AssigneeId.Value,
                        "Giao việc",
                        $"Bạn vừa được gán công việc: '{cv.TieuDe}' thông qua việc chỉnh sửa."
                    );
                }
            }

            return MapToDto(cv);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var cv = await _repository.GetByIdAsync(id);
            if (cv == null) return false;

            int duAnId = cv.DuAnId;
            var result = await _repository.DeleteAsync(id);
            if (result)
            {
                await _notificationService.NotifyTaskUpdated(duAnId);
            }
            return result;
        }

        public async Task<bool> UpdateStatusAsync(int id, TrangThaiCongViec status, int updaterId)
        {
            var cv = await _repository.GetByIdAsync(id);
            if (cv == null) return false;

            // Ràng buộc nghiệp vụ: Chặn mọi trạng thái "sau Todo" nếu Dependency chưa xong
            if (status != TrangThaiCongViec.Todo && status != TrangThaiCongViec.Cancelled)
            {
                await CheckDependenciesAsync(cv);
            }

            // Ràng buộc nghiệp vụ: Chỉ người giao việc (người tạo task) mới được phép chuyển sang trạng thái Done
            if (status == TrangThaiCongViec.Done && cv.CreatedBy != updaterId)
            {
                throw new Exception("Chỉ người giao việc mới có quyền hoàn thành công việc này.");
            }

            // Ràng buộc nghiệp vụ: Chỉ cho phép cập nhật trạng thái nếu Sprint đang InProgress (hoặc không thuộc Sprint nào)
            if (cv.SprintId.HasValue && cv.Sprint != null)
            {
                var now = DateTime.UtcNow;
                bool isWorkable = cv.Sprint.TrangThai == TrangThaiSprint.InProgress || 
                                 (cv.Sprint.TrangThai == TrangThaiSprint.New && now >= cv.Sprint.NgayBatDau && now <= cv.Sprint.NgayKetThuc);
                
                if (!isWorkable && status != TrangThaiCongViec.Todo && status != TrangThaiCongViec.Cancelled)
                {
                    return false; 
                }
            }
 
            // Phát hiện từ chối: Nếu đang ở Review mà bị đẩy về Todo/InProgress
            if (cv.TrangThai == TrangThaiCongViec.Review && 
                (status == TrangThaiCongViec.Todo || status == TrangThaiCongViec.InProgress))
            {
                cv.SoLanBiTuChoi++;
                
                // Cảnh báo nếu bị từ chối nhiều lần (ví dụ >= 3)
                if (cv.SoLanBiTuChoi >= 3 && cv.AssigneeId.HasValue)
                {
                    await _notificationService.NotifyPersonal(
                        cv.AssigneeId.Value, 
                        "Cảnh báo hiệu suất", 
                        $"Công việc '{cv.TieuDe}' đã bị từ chối {cv.SoLanBiTuChoi} lần. Vui lòng kiểm tra lại chất lượng đầu ra!"
                    );
                }
            }
 
            cv.TrangThai = status;

            // Tự động ghi nhận ngày bắt đầu thực tế và tính ngày kết thúc dự kiến
            if (status == TrangThaiCongViec.InProgress && cv.NgayBatDauThucTe == null)
            {
                cv.NgayBatDauThucTe = DateTime.UtcNow;

                // Tính NgayKetThucDuKien = NgayBatDauThucTe + ThoiGianUocTinh (ngày làm việc)
                // Ví dụ: 8h = 1 ngày, 16h = 2 ngày, bỏ qua T7/CN
                cv.NgayKetThucDuKien = TinhNgayKetThucDuKien(cv.NgayBatDauThucTe.Value, cv.ThoiGianUocTinh);
            }

            // Tự động ghi nhận ngày kết thúc thực tế và giảm khối lượng công việc cho User
            if (status == TrangThaiCongViec.Done)
            {
                cv.NgayKetThucThucTe = DateTime.UtcNow;
                if (cv.AssigneeId.HasValue)
                {
                    var user = await _userRepository.LayTheoIdAsync(cv.AssigneeId.Value);
                    if (user != null)
                    {
                        user.KhoiLuongCongViec = Math.Max(0, user.KhoiLuongCongViec - cv.ThoiGianUocTinh);
                        await _userRepository.UpdateAsync(user);
                    }
                }
            }

            var result = await _repository.UpdateAsync(cv);
            if (result)
            {
                await _notificationService.NotifyTaskUpdated(cv.DuAnId);
                
                // Fix #9: Nếu task hoàn thành, kiểm tra xem Sprint có hoàn thành theo không
                if (status == TrangThaiCongViec.Done && cv.SprintId.HasValue)
                {
                    await CheckAndCompleteSprintAsync(cv.SprintId.Value);
                }

                // Thông báo cho người tạo/quản lý
                if (cv.CreatedBy.HasValue)
                {
                    await _notificationService.NotifyPersonal(cv.CreatedBy.Value, "Cập nhật công việc", $"Công việc '{cv.TieuDe}' vừa được đổi trạng thái sang {status}");
                }
                
                // Thông báo cho người thực hiện (nếu có và không phải là người đổi/người tạo đã nhận thông báo trên)
                if (cv.AssigneeId.HasValue && cv.AssigneeId != cv.CreatedBy)
                {
                    await _notificationService.NotifyPersonal(cv.AssigneeId.Value, "Trạng thái công việc", $"Công việc '{cv.TieuDe}' của bạn đã đổi sang {status}");
                }
            }
            return result;
        }

        public async Task<bool> CapNhatTienDoAsync(int id, CapNhatTienDoDto dto, int updaterId)
        {
            var cv = await _repository.GetByIdAsync(id);
            if (cv == null) return false;

            // Ràng buộc nghiệp vụ: Chặn mọi trạng thái "sau Todo" nếu Dependency chưa xong
            if (dto.TrangThai != TrangThaiCongViec.Todo && dto.TrangThai != TrangThaiCongViec.Cancelled)
            {
                await CheckDependenciesAsync(cv);
            }

            // Ràng buộc nghiệp vụ: Chỉ người giao việc mới được phép chuyển sang trạng thái Done
            if (dto.TrangThai == TrangThaiCongViec.Done && cv.CreatedBy != updaterId)
            {
                throw new Exception("Chỉ người giao việc mới có quyền hoàn thành công việc này.");
            }

            // Ràng buộc nghiệp vụ: Tương tự UpdateStatus
            if (cv.SprintId.HasValue && cv.Sprint != null)
            {
                var now = DateTime.UtcNow;
                bool isWorkable = cv.Sprint.TrangThai == TrangThaiSprint.InProgress || 
                                 (cv.Sprint.TrangThai == TrangThaiSprint.New && now >= cv.Sprint.NgayBatDau && now <= cv.Sprint.NgayKetThuc);

                if (!isWorkable && dto.TrangThai != TrangThaiCongViec.Todo && dto.TrangThai != TrangThaiCongViec.Cancelled)
                {
                    return false;
                }
            }

            // Phát hiện từ chối: Nếu đang ở Review mà bị trả về trạng thái trước đó
            if (cv.TrangThai == TrangThaiCongViec.Review && 
                (dto.TrangThai == TrangThaiCongViec.Todo || dto.TrangThai == TrangThaiCongViec.InProgress))
            {
                cv.SoLanBiTuChoi++;
                if (cv.SoLanBiTuChoi >= 3 && cv.AssigneeId.HasValue)
                {
                    await _notificationService.NotifyPersonal(
                        cv.AssigneeId.Value, 
                        "Cảnh báo hiệu suất", 
                        $"Công việc '{cv.TieuDe}' đã bị từ chối {cv.SoLanBiTuChoi} lần (qua cập nhật tiến độ)."
                    );
                }
            }

            // 1. Cập nhật trạng thái và thời gian thực tế của Task
            cv.TrangThai = dto.TrangThai;
            cv.ThoiGianThucTe = (cv.ThoiGianThucTe ?? 0) + (dto.ThoiGianLamViecThem > 0 ? dto.ThoiGianLamViecThem : 0);

            // Tự động gán ngày kết thúc và giảm khối lượng công việc nếu hoàn thành
            if (dto.TrangThai == TrangThaiCongViec.Done)
            {
                cv.NgayKetThucThucTe = DateTime.UtcNow;
                if (cv.AssigneeId.HasValue)
                {
                    var user = await _userRepository.LayTheoIdAsync(cv.AssigneeId.Value);
                    if (user != null)
                    {
                        user.KhoiLuongCongViec = Math.Max(0, user.KhoiLuongCongViec - cv.ThoiGianUocTinh);
                        await _userRepository.UpdateAsync(user);
                    }
                }
            }
           
            // Tự động gán ngày bắt đầu và tính ngày kết thúc dự kiến nếu mới bắt đầu
            if (dto.TrangThai == TrangThaiCongViec.InProgress && cv.NgayBatDauThucTe == null)
            {
                cv.NgayBatDauThucTe = DateTime.UtcNow;

                // Tính NgayKetThucDuKien = NgayBatDauThucTe + ThoiGianUocTinh (ngày làm việc)
                cv.NgayKetThucDuKien = TinhNgayKetThucDuKien(cv.NgayBatDauThucTe.Value, cv.ThoiGianUocTinh);
            }

            // 2. Tạo bản ghi nhật ký (Task Log)
            var log = new NhatKyCongViec
            {
                CongViecId = id,
                NguoiCapNhatId = updaterId,
                SoGioLamViec = dto.ThoiGianLamViecThem,
                GhiChu = dto.GhiChu ?? $"Cập nhật trạng thái sang {dto.TrangThai}",
                NgayCapNhat = DateTime.UtcNow
            };

            await _taskLogRepository.AddAsync(log);

            // 3. Lưu Task
            var result = await _repository.UpdateAsync(cv);
            if (result)
            {
                await _notificationService.NotifyTaskUpdated(cv.DuAnId);
                
                // Fix #9: Nếu task hoàn thành, kiểm tra hoàn thành Sprint
                if (dto.TrangThai == TrangThaiCongViec.Done && cv.SprintId.HasValue)
                {
                    await CheckAndCompleteSprintAsync(cv.SprintId.Value);
                }

                // Thông báo cho người quản lý
                if (cv.CreatedBy.HasValue)
                {
                    await _notificationService.NotifyPersonal(cv.CreatedBy.Value, "Cập nhật tiến độ", $"Công việc '{cv.TieuDe}' vừa được cập nhật tiến độ.");
                }
            }
            return result;
        }

        public async Task<bool> GiaoViecThuCongAsync(GiaoViecThuCongDto dto, int assignerId)
        {
            var cv = await _repository.GetByIdAsync(dto.CongViecId);
            if (cv == null) return false;

            if (dto.AssigneeId == 0)
            {
                // Logic hủy gán việc (Gỡ phân công)
                if (cv.AssigneeId.HasValue)
                {
                    var oldUser = await _userRepository.LayTheoIdAsync(cv.AssigneeId.Value);
                    if (oldUser != null)
                    {
                        oldUser.KhoiLuongCongViec = Math.Max(0, oldUser.KhoiLuongCongViec - cv.ThoiGianUocTinh);
                        await _userRepository.UpdateAsync(oldUser);
                    }
                }
                cv.AssigneeId = null;
                cv.PhuongThucGiaoViec = PhuongThucGiaoViec.Manual;
                var res = await _repository.UpdateAsync(cv);
                if (res) await _notificationService.NotifyTaskUpdated(cv.DuAnId);
                return res;
            }

            var user = await _userRepository.LayTheoIdAsync(dto.AssigneeId);
            if (user == null) return false;

            // Ghi nhận lịch sử giao việc
            var log = new NhatKyCongViec
            {
                CongViecId = dto.CongViecId,
                NguoiCapNhatId = assignerId,
                SoGioLamViec = 0,
                GhiChu = $"Giao việc cho nhân viên: {user.FullName}",
                NgayCapNhat = DateTime.UtcNow
            };
            await _taskLogRepository.AddAsync(log);

            // Giảm khối lượng công việc của Assignee cũ (nếu có)
            if (cv.AssigneeId.HasValue)
            {
                var oldUser = await _userRepository.LayTheoIdAsync(cv.AssigneeId.Value);
                if (oldUser != null)
                {
                    oldUser.KhoiLuongCongViec = Math.Max(0, oldUser.KhoiLuongCongViec - cv.ThoiGianUocTinh);
                    await _userRepository.UpdateAsync(oldUser);
                }
            }

            // Tăng khối lượng công việc của Assignee mới
            user.KhoiLuongCongViec += cv.ThoiGianUocTinh;
            await _userRepository.UpdateAsync(user);

            cv.AssigneeId = dto.AssigneeId;
            cv.PhuongThucGiaoViec = PhuongThucGiaoViec.Manual; 
            cv.SoLanBiTuChoi = 0; // Reset phạt khi giao cho người mới

            var result = await _repository.UpdateAsync(cv);
            if (result)
            {
                await _notificationService.NotifyTaskUpdated(cv.DuAnId);
                
                // Thông báo cho người được gán mới
                await _notificationService.NotifyPersonal(dto.AssigneeId, "Giao việc mới", $"Bạn vừa được giao công việc: {cv.TieuDe}");
            }
            return result;
        }

        public async Task<IEnumerable<CongViecDto>> GetTasksPendingReviewAsync()
        {
            var query = new Apllication.DTOs.CongViecQueryDto
            {
                TrangThai = (int)TrangThaiCongViec.Review,
                PageSize = 100 // Lấy tối đa 100 task chờ duyệt
            };
            
            var result = await _repository.LayDanhSachCongViecAsync(query);
            return (result?.Items ?? Enumerable.Empty<CongViec>()).Select(MapToDto);
        }

        private CongViecDto MapToDto(CongViec cv)
        {
            return new CongViecDto
            {
                Id = cv.Id,
                DuAnId = cv.DuAnId,
                SprintId = cv.SprintId,
                TieuDe = cv.TieuDe,
                MoTa = cv.MoTa,
                LoaiCongViec = cv.LoaiCongViec,
                DoUuTien = cv.DoUuTien,
                TrangThai = cv.TrangThai,
                AssigneeId = cv.AssigneeId,
                AssigneeName = cv.Assignee?.FullName,
                PhuongThucGiaoViec = cv.PhuongThucGiaoViec,
                ThoiGianUocTinh = cv.ThoiGianUocTinh,
                ThoiGianThucTe = cv.ThoiGianThucTe,
                NgayBatDauDuKien = cv.NgayBatDauDuKien,
                NgayKetThucDuKien = cv.NgayKetThucDuKien,
                NgayBatDauThucTe = cv.NgayBatDauThucTe,
                NgayKetThucThucTe = cv.NgayKetThucThucTe,
                SprintStatus = cv.Sprint?.TrangThai,
                NgayBatDauSprint = cv.Sprint?.NgayBatDau,
                NgayKetThucSprint = cv.Sprint?.NgayKetThuc,
                SoLanBiTuChoi = cv.SoLanBiTuChoi,
                CreatedBy = cv.CreatedBy,
                Dependencies = cv.Dependencies?.Select(d => new PhuThuocDto
                {
                    DependsOnTaskId = d.DependsOnTaskId,
                    DependsOnTaskTitle = d.DependsOnTask?.TieuDe
                }).ToList() ?? new List<PhuThuocDto>()
            };
        }

        /// <summary>
        /// Fix #9: Kiểm tra và tự động hoàn thành Sprint nếu tất cả task đã Done.
        /// </summary>
        private async Task CheckAndCompleteSprintAsync(int sprintId)
        {
            var sprint = await _sprintRepository.GetByIdAsync(sprintId);
            if (sprint == null || sprint.TrangThai == TrangThaiSprint.Finished) return;

            // Lấy danh sách task thuộc sprint này (dùng repository có sẵn filter dự án / query)
            // Lưu ý: ICongViecRepository chỉ có GetByProjectIdAsync, chưa có GetBySprintIdAsync cụ thể
            // nhưng CongViecRepository.LayDanhSachCongViecAsync có thể dùng query.
            // Để đơn giản và chính xác, ta lọc trên sprint.CongViecs nếu repository đã Include sẵn.
            // SprintRepository.GetByIdAsync thường Include CongViecs.
            
            if (sprint.CongViecs != null && sprint.CongViecs.Any())
            {
                bool allDone = sprint.CongViecs.All(c => c.TrangThai == TrangThaiCongViec.Done || c.TrangThai == TrangThaiCongViec.Cancelled);
                if (allDone)
                {
                    sprint.TrangThai = TrangThaiSprint.Finished;
                    await _sprintRepository.UpdateAsync(sprint);
                }
            }
        }

        private async Task CheckDependenciesAsync(CongViec task)
        {
            if (task.Dependencies != null && task.Dependencies.Any())
            {
                foreach (var dep in task.Dependencies)
                {
                    var predecessor = await _repository.GetByIdAsync(dep.DependsOnTaskId);
                    if (predecessor != null && predecessor.TrangThai != TrangThaiCongViec.Done)
                    {
                        var sprintInfo = predecessor.Sprint != null ? $"trong '{predecessor.Sprint.TenSprint}' " : "";
                        throw new Exception($"Không thể thực hiện! Bạn phải chờ Task '{predecessor.TieuDe}' (#{(predecessor.Id)}) {sprintInfo}hoàn thành trước.");
                    }
                }
            }
        }

        /// <summary>
        /// Tính ngày kết thúc dự kiến dựa trên ngày bắt đầu và số giờ ước tính.
        /// </summary>
        private DateTime TinhNgayKetThucDuKien(DateTime start, double hours)
        {
            int soNgayLamViec = (int)Math.Ceiling(hours / 8.0);
            DateTime result = start.Date;
            int ngayDaDem = 0;
            while (ngayDaDem < soNgayLamViec)
            {
                if (result.DayOfWeek != DayOfWeek.Saturday && result.DayOfWeek != DayOfWeek.Sunday) ngayDaDem++;
                if (ngayDaDem < soNgayLamViec) result = result.AddDays(1);
            }
            return result;
        }
    }
}
