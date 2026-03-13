using Apllication.DTOs.CongViec;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apllication.IService
{
    public interface ICongViecService
    {
        Task<CongViecDto> GetByIdAsync(int id);
        Task<IEnumerable<CongViecDto>> GetByProjectIdAsync(int projectId);
        Task<CongViecDto> CreateAsync(TaoCongViecDto dto);
        Task<bool> UpdateStatusAsync(int id, string status);
        
        /// <summary>
        /// Luồng giao việc thủ công từ PM đến nhân viên.
        /// </summary>
        Task<bool> GiaoViecThuCongAsync(GiaoViecThuCongDto dto);
    }
}
