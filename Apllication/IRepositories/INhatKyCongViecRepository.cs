using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apllication.IRepositories
{
    public interface INhatKyCongViecRepository
    {
        Task<NhatKyCongViec> AddAsync(NhatKyCongViec nhatKy);
        Task<IEnumerable<NhatKyCongViec>> GetByTaskIdAsync(int taskId);
        Task<IEnumerable<NhatKyCongViec>> GetByProjectIdAsync(int projectId);
    }
}
