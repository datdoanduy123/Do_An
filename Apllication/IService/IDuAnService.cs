using Apllication.DTOs.DuAn;
using Domain.Entities;
using Domain.Enums;
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
        Task<bool> AddMemberAsync(int duAnId, int userId, ProjectRole role = ProjectRole.Member);
        Task<bool> UpdateMemberRoleAsync(int duAnId, int userId, ProjectRole newRole);
        Task<bool> RemoveMemberAsync(int duAnId, int userId);
        Task<IEnumerable<object>> GetSkillCoverageAsync(int projectId);
    }
}
