using Apllication.DTOs.QuyTacGiaoViecAI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apllication.IService
{
    public interface IQuyTacGiaoViecAIService
    {
        Task<IEnumerable<QuyTacGiaoViecAIDto>> GetAllAsync();
        Task<QuyTacGiaoViecAIDto?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(int id, CapNhatQuyTacGiaoViecAIDto dto);
    }
}
