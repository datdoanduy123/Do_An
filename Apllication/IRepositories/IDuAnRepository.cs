using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apllication.IRepositories
{
    public interface IDuAnRepository
    {
        Task<DuAn?> GetByIdAsync(int id);
        Task<IEnumerable<DuAn>> GetAllAsync();
        Task<DuAn> AddAsync(DuAn duAn);
        Task<bool> UpdateAsync(DuAn duAn);
        Task<bool> DeleteAsync(int id);
    }
}
