using Apllication.DTOs.NhatKyCongViec;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apllication.IService
{
    public interface INhatKyCongViecService
    {
        Task<NhatKyDto> AddLogAsync(int taskId, int userId, double hours, string? note);
        Task<IEnumerable<NhatKyDto>> GetLogsByTaskIdAsync(int taskId);
    }
}
