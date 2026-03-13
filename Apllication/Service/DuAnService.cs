using Apllication.DTOs.DuAn;
using Apllication.IRepositories;
using Apllication.IService;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Apllication.Service
{
    public class DuAnService : IDuAnService
    {
        private readonly IDuAnRepository _repository;

        public DuAnService(IDuAnRepository repository)
        {
            _repository = repository;
        }

        public async Task<DuAnDto> GetByIdAsync(int id)
        {
            var duAn = await _repository.GetByIdAsync(id);
            if (duAn == null) return null!;

            return new DuAnDto
            {
                Id = duAn.Id,
                TenDuAn = duAn.TenDuAn,
                MoTa = duAn.MoTa,
                NgayBatDau = duAn.NgayBatDau,
                NgayKetThuc = duAn.NgayKetThuc,
                TrangThai = duAn.TrangThai,
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

        public async Task<DuAnDto> CreateAsync(TaoDuAnDto dto)
        {
            var duAn = new DuAn
            {
                TenDuAn = dto.TenDuAn,
                MoTa = dto.MoTa,
                NgayBatDau = dto.NgayBatDau,
                NgayKetThuc = dto.NgayKetThuc,
                TrangThai = "Planning",
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
    }
}
