using Apllication.DTOs.DuAn;
using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apllication.IService
{
    public interface IDuAnService
    {
        Task<DuAnDto?> GetByIdAsync(int id);
        Task<IEnumerable<DuAnDto>> GetAllAsync();
        Task<DuAnDto> CreateAsync(TaoDuAnDto dto, int creatorId);
        Task<bool> UpdateAsync(int id, CapNhatDuAnDto dto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<ThanhVienDuAnDto>> GetMembersAsync(int id);
        Task<bool> AddMemberAsync(int duAnId, int userId);
        Task<bool> RemoveMemberAsync(int id, int userId);
        Task<IEnumerable<object>> GetSkillCoverageAsync(int projectId);
    }
}
