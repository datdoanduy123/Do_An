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
        /// Từ chối công việc và yêu cầu làm lại với lý do cụ thể.
        /// </summary>
        Task<bool> RejectTaskWithReasonAsync(int id, string reason, int rejectorId);

        /// <summary>
        /// Thêm bình luận/thảo luận vào công việc.
        /// </summary>
        Task<TraoLoiDto> AddCommentAsync(int taskId, TaoTraoLoiDto dto, int creatorId);

        /// <summary>
        /// Lấy danh sách thảo luận của công việc.
        /// </summary>
        Task<IEnumerable<TraoLoiDto>> GetCommentsAsync(int taskId);

        /// <summary>
        /// Luồng giao việc thủ công từ PM đến nhân viên.
        /// </summary>
        Task<bool> GiaoViecThuCongAsync(GiaoViecThuCongDto dto, int assignerId);
        Task<IEnumerable<CongViecDto>> GetTasksPendingReviewAsync();
    }
}
