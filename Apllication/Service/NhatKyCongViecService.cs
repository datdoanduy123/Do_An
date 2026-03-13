using Apllication.DTOs.NhatKyCongViec;
using Apllication.IRepositories;
using Apllication.IService;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Apllication.Service
{
    public class NhatKyCongViecService : INhatKyCongViecService
    {
        private readonly INhatKyCongViecRepository _repository;
        private readonly ICongViecRepository _taskRepository;

        public NhatKyCongViecService(INhatKyCongViecRepository repository, ICongViecRepository taskRepository)
        {
            _repository = repository;
            _taskRepository = taskRepository;
        }

        public async Task<NhatKyDto> AddLogAsync(int taskId, int userId, double hours, string? note)
        {
            var nhatKy = new NhatKyCongViec
            {
                CongViecId = taskId,
                NguoiCapNhatId = userId,
                SoGioLamViec = hours,
                GhiChu = note,
                NgayCapNhat = DateTime.UtcNow
            };

            var ketQua = await _repository.AddAsync(nhatKy);
            return new NhatKyDto
            {
                Id = ketQua.Id,
                CongViecId = ketQua.CongViecId,
                NguoiCapNhatId = ketQua.NguoiCapNhatId,
                SoGioLamViec = ketQua.SoGioLamViec,
                GhiChu = ketQua.GhiChu,
                NgayCapNhat = ketQua.NgayCapNhat
            };
        }

        public async Task<IEnumerable<NhatKyDto>> GetLogsByTaskIdAsync(int taskId)
        {
            var logs = await _repository.GetByTaskIdAsync(taskId);
            return logs.Select(l => new NhatKyDto
            {
                Id = l.Id,
                CongViecId = l.CongViecId,
                NguoiCapNhatId = l.NguoiCapNhatId,
                TenNguoiCapNhat = l.NguoiCapNhat?.FullName,
                SoGioLamViec = l.SoGioLamViec,
                GhiChu = l.GhiChu,
                NgayCapNhat = l.NgayCapNhat
            });
        }
    }
}
