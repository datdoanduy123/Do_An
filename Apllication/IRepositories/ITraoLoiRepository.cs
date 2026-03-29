using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apllication.IRepositories
{
    public interface ITraoLoiRepository
    {
        Task<TraoLoiCongViec> GetByIdAsync(int id);
        Task<IEnumerable<TraoLoiCongViec>> GetByCongViecIdAsync(int congViecId);
        Task<TraoLoiCongViec> AddAsync(TraoLoiCongViec traoLoi);
        Task<bool> DeleteAsync(int id);
    }
}
