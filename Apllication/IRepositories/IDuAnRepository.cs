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
        Task<IEnumerable<DuAnNguoiDung>> GetMembersAsync(int duAnId);
        Task<bool> AddMemberAsync(int duAnId, int userId);
        Task<bool> RemoveMemberAsync(int duAnId, int userId);
    }
}
