using Apllication.DTOs.CongViec;
using Domain.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apllication.IService
{
    public interface ICongViecService
    {
        Task<CongViecDto> GetByIdAsync(int id);
        Task<IEnumerable<CongViecDto>> GetByProjectIdAsync(int projectId);
        Task<CongViecDto> CreateAsync(TaoCongViecDto dto, int creatorId);
        Task<bool> UpdateStatusAsync(int id, TrangThaiCongViec status);
        Task<bool> CapNhatTienDoAsync(int id, CapNhatTienDoDto dto, int updaterId);
        
        /// <summary>
        /// Luồng giao việc thủ công từ PM đến nhân viên.
        /// </summary>
        Task<bool> GiaoViecThuCongAsync(GiaoViecThuCongDto dto, int assignerId);
    }
}
