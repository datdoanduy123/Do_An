using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apllication.IRepositories
{
    public interface ICongViecRepository
    {
        Task<CongViec?> GetByIdAsync(int id);
        Task<IEnumerable<CongViec>> GetByProjectIdAsync(int projectId);
        Task<CongViec> AddAsync(CongViec congViec);
        Task<bool> UpdateAsync(CongViec congViec);
    }
}
