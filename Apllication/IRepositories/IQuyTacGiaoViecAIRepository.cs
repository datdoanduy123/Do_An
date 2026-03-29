using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apllication.IRepositories
{
    public interface IQuyTacGiaoViecAIRepository
    {
        Task<IEnumerable<QuyTacGiaoViecAI>> GetAllAsync();
        Task<QuyTacGiaoViecAI?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(QuyTacGiaoViecAI rule);
        Task<IEnumerable<QuyTacGiaoViecAI>> GetAllActiveRulesAsync();
        Task<QuyTacGiaoViecAI?> GetByCodeAsync(string code);
    }
}
