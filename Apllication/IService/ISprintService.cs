using Apllication.DTOs.Sprint;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apllication.IService
{
    public interface ISprintService
    {
        Task<SprintDto> GetByIdAsync(int id);
        Task<IEnumerable<SprintDto>> GetByProjectIdAsync(int projectId);
        Task<SprintDto> CreateAsync(TaoSprintDto dto, int creatorId);
        Task<bool> UpdateAsync(int id, CapNhatSprintDto dto);
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Kích hoạt Sprint: Đổi trạng thái từ New → InProgress.
        /// Cập nhật NgayBatDau = ngày hiện tại, NgayKetThuc = NgayBatDau + 14 ngày.
        /// Hỗ trợ nhiều Sprint kích hoạt song song (nhiều team làm việc cùng lúc).
        /// </summary>
        Task<SprintDto?> KichHoatSprintAsync(int sprintId, int userId);
    }
}
