using Apllication.DTOs.QuyTacGiaoViecAI;
using Apllication.IRepositories;
using Apllication.IService;
using Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Apllication.Service
{
    public class QuyTacGiaoViecAIService : IQuyTacGiaoViecAIService
    {
        private readonly IQuyTacGiaoViecAIRepository _repository;

        public QuyTacGiaoViecAIService(IQuyTacGiaoViecAIRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<QuyTacGiaoViecAIDto>> GetAllAsync()
        {
            var rules = await _repository.GetAllAsync();
            return rules.Select(MapToDto);
        }

        public async Task<QuyTacGiaoViecAIDto?> GetByIdAsync(int id)
        {
            var rule = await _repository.GetByIdAsync(id);
            if (rule == null) return null;
            return MapToDto(rule);
        }

        public async Task<bool> UpdateAsync(int id, CapNhatQuyTacGiaoViecAIDto dto)
        {
            var rule = await _repository.GetByIdAsync(id);
            if (rule == null) return false;

            rule.GiaTri = dto.GiaTri;
            rule.IsActive = dto.IsActive;

            return await _repository.UpdateAsync(rule);
        }

        private QuyTacGiaoViecAIDto MapToDto(QuyTacGiaoViecAI rule)
        {
            return new QuyTacGiaoViecAIDto
            {
                Id = rule.Id,
                MaQuyTac = rule.MaQuyTac,
                GiaTri = rule.GiaTri,
                LoaiDuLieu = rule.LoaiDuLieu,
                MoTa = rule.MoTa,
                IsActive = rule.IsActive
            };
        }
    }
}
