using Domain.Entities;
using Domain.Enums;
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
        Task<bool> AddMemberAsync(int duAnId, int userId, ProjectRole role = ProjectRole.Member);
        Task<bool> UpdateMemberRoleAsync(int duAnId, int userId, ProjectRole newRole);
        Task<bool> RemoveMemberAsync(int duAnId, int userId);
    }
}
