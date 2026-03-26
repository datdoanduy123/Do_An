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
        Task<IEnumerable<CongViecDto>> GetMyTasksAsync(int userId);
        Task<CongViecDto> CreateAsync(TaoCongViecDto dto, int creatorId);
        Task<CongViecDto> UpdateAsync(int id, CapNhatCongViecDto dto);
        Task<bool> DeleteAsync(int id);
        Task<bool> UpdateStatusAsync(int id, TrangThaiCongViec status, int updaterId);
        Task<bool> CapNhatTienDoAsync(int id, CapNhatTienDoDto dto, int updaterId);
        
        /// <summary>
        /// Luồng giao việc thủ công từ PM đến nhân viên.
        Task<bool> GiaoViecThuCongAsync(GiaoViecThuCongDto dto, int assignerId);
        Task<IEnumerable<CongViecDto>> GetTasksPendingReviewAsync();
    }
}
