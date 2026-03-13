using Apllication.DTOs.CongViec;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apllication.IService
{
    public interface IGiaoViecAIService
    {
        /// <summary>
        /// Gợi ý danh sách nhân viên phù hợp nhất cho một công việc.
        /// </summary>
        Task<IEnumerable<GoiYGiaoViecDto>> GoiYAssigneeAsync(int congViecId);

        /// <summary>
        /// Tự động giao việc cho toàn bộ backlog hoặc dự án (Sử dụng cho luồng AI Auto-Assign).
        /// </summary>
        Task<bool> TuDongGiaoViecDuAnAsync(int duAnId);
    }
}
