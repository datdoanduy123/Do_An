using Apllication.DTOs.TaiLieuDuAn;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apllication.IService
{
    public interface ITaiLieuDuAnService
    {
        /// <summary>
        /// Tải tài liệu lên hệ thống.
        /// </summary>
        Task<TaiLieuDuAnDto> UploadAsync(int duAnId, IFormFile file, int userId);

        /// <summary>
        /// Kích hoạt AI xử lý tài liệu này để sinh công việc.
        /// </summary>
        Task<bool> ProcessWithAiAsync(int taiLieuId);

        Task<IEnumerable<TaiLieuDuAnDto>> GetByProjectIdAsync(int projectId);
    }
}
