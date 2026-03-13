using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apllication.IRepositories
{
    public interface ISprintRepository
    {
        Task<Sprint?> GetByIdAsync(int id);
        Task<IEnumerable<Sprint>> GetByProjectIdAsync(int projectId);
        Task<Sprint> AddAsync(Sprint sprint);
        Task<bool> UpdateAsync(Sprint sprint);
        Task<bool> DeleteAsync(int id);
    }
}
