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

        // Các phương thức cho Phân cấp
        Task<IEnumerable<NhomKyNang>> GetAllNhomKyNangAsync();
        Task<IEnumerable<CongNghe>> GetCongNgheByNhomAsync(int nhomId);
        Task<IEnumerable<NhomKyNang>> GetHierarchyAsync();
        Task AddNhomAsync(NhomKyNang nhom);
        Task AddCongNgheAsync(CongNghe cn);
        Task<NhomKyNang?> GetNhomByIdAsync(int id);
        Task<CongNghe?> GetCongNgheByIdAsync(int id);
    }
}
