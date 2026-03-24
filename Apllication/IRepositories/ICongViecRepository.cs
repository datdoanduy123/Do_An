using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apllication.IRepositories
{
    public interface ICongViecRepository
    {
        Task<CongViec?> GetByIdAsync(int id);
        Task<IEnumerable<CongViec>> GetByProjectIdAsync(int projectId);
        Task<IEnumerable<CongViec>> GetByAssigneeIdAsync(int assigneeId);
        Task<IEnumerable<CongViec>> GetAllAsync();
        Task<CongViec> AddAsync(CongViec congViec);
        Task<bool> UpdateAsync(CongViec congViec);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<CongViec>> GetTasksWithRequirementsByProjectAsync(int projectId);
        Task<Apllication.DTOs.PagedResultDto<CongViec>> LayDanhSachCongViecAsync(Apllication.DTOs.CongViecQueryDto query);
    }
}
