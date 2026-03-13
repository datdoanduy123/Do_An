using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apllication.IRepositories
{
    public interface IQuyTacGiaoViecAIRepository
    {
        Task<IEnumerable<QuyTacGiaoViecAI>> GetAllActiveRulesAsync();
        Task<QuyTacGiaoViecAI?> GetByCodeAsync(string code);
    }
}
