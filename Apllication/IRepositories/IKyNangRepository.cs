using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apllication.IRepositories
{
    public interface IKyNangRepository
    {
        Task<IEnumerable<KyNang>> GetAllAsync();
        Task<KyNang?> GetByNameAsync(string name);
    }
}
