using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apllication.IRepositories
{
    public interface ITaiLieuDuAnRepository
    {
        Task<TaiLieuDuAn?> GetByIdAsync(int id);
        Task<IEnumerable<TaiLieuDuAn>> GetByProjectIdAsync(int projectId);
        Task<TaiLieuDuAn> AddAsync(TaiLieuDuAn taiLieu);
        Task<bool> UpdateAsync(TaiLieuDuAn taiLieu);
        Task<bool> DeleteAsync(int id);
    }
}
