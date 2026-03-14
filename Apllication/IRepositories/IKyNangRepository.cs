using Apllication.DTOs;
using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apllication.IRepositories
{
    public interface IKyNangRepository
    {
        Task<IEnumerable<KyNang>> GetAllAsync();
        Task<KyNang?> GetByNameAsync(string name);
        Task<KetQuaPhanTrangDto<KyNangDto>> LayDanhSachKyNangAsync(KyNangQueryDto query);
        Task<KyNang?> GetByIdAsync(int id);
        Task AddAsync(KyNang kyNang);
        Task UpdateAsync(KyNang kyNang);
        Task DeleteAsync(KyNang kyNang);
        Task SaveChangesAsync();
    }
}
