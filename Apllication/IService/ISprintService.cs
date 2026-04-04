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
        /// Cập nhật NgayBatDau = ngày hiện tại, NgayKetThuc = NgayBatDau + số ngày do QuyTacGiaoViecAI quy định.
        /// Ràng buộc: Mỗi dự án chỉ được có DUY NHẤT 1 Sprint đang InProgress tại một thời điểm.
        /// </summary>
        Task<SprintDto?> KichHoatSprintAsync(int sprintId, int userId);

        /// <summary>
        /// Tự động mở Sprint tiếp theo (trạng thái New, NgayBatDau nhỏ nhất) trong cùng dự án
        /// khi Sprint vừa hoàn thành (Finished). Được gọi nội bộ bởi CongViecService.
        /// </summary>
        Task TuDongMoSprintTiepTheoAsync(int completedSprintId);
    }
}
