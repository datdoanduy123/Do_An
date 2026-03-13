using Apllication.DTOs;
using Apllication.DTOs.DuAn;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apllication.IService
{
    public interface IDuAnService
    {
        Task<DuAnDto> GetByIdAsync(int id);
        Task<IEnumerable<DuAnDto>> GetAllAsync();
        Task<DuAnDto> CreateAsync(TaoDuAnDto dto);
        Task<bool> UpdateAsync(int id, CapNhatDuAnDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
