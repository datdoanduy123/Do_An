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

        public CongViecService(
            ICongViecRepository repository, 
            INguoiDungRepository userRepository,
            INhatKyCongViecRepository taskLogRepository)
        {
            _repository = repository;
            _userRepository = userRepository;
            _taskLogRepository = taskLogRepository;
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
                StoryPoints = dto.StoryPoints,
                ThoiGianUocTinh = dto.ThoiGianUocTinh,
                NgayBatDauDuKien = dto.NgayBatDauDuKien,
                NgayKetThucDuKien = dto.NgayKetThucDuKien,
                PhuongThucGiaoViec = PhuongThucGiaoViec.Manual,
                CreatedBy = creatorId,
                CreatedAt = DateTime.UtcNow
            };

            var ketQua = await _repository.AddAsync(cv);
            return MapToDto(ketQua);
        }

        public async Task<bool> UpdateStatusAsync(int id, TrangThaiCongViec status)
        {
            var cv = await _repository.GetByIdAsync(id);
            if (cv == null) return false;

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

            cv.TrangThai = status;

            // Tự động ghi nhận ngày bắt đầu thực tế
            if (status == TrangThaiCongViec.InProgress && cv.NgayBatDauThucTe == null)
            {
                cv.NgayBatDauThucTe = DateTime.UtcNow;
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

            return await _repository.UpdateAsync(cv);
        }

        public async Task<bool> CapNhatTienDoAsync(int id, CapNhatTienDoDto dto, int updaterId)
        {
            var cv = await _repository.GetByIdAsync(id);
            if (cv == null) return false;

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
           
            // Tự động gán ngày bắt đầu nếu mới bắt đầu
            if (dto.TrangThai == TrangThaiCongViec.InProgress && cv.NgayBatDauThucTe == null)
            {
                cv.NgayBatDauThucTe = DateTime.UtcNow;
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
            return await _repository.UpdateAsync(cv);
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
                return await _repository.UpdateAsync(cv);
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

            return await _repository.UpdateAsync(cv);
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
                StoryPoints = cv.StoryPoints,
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
                NgayKetThucSprint = cv.Sprint?.NgayKetThuc
            };
        }
    }
}
