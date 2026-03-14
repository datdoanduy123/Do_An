using Apllication.DTOs;
using Apllication.IRepositories;
using Apllication.IService;
using Domain.Entities;

namespace Apllication.Service
{
    public class KyNangService : IKyNangService
    {
        private readonly IKyNangRepository _kyNangRepository;

        public KyNangService(IKyNangRepository kyNangRepository)
        {
            _kyNangRepository = kyNangRepository;
        }

        public async Task<KetQuaPhanTrangDto<KyNangDto>> LayDanhSachKyNangAsync(KyNangQueryDto query)
        {
            return await _kyNangRepository.LayDanhSachKyNangAsync(query);
        }

        public async Task<KyNangDto?> LayTheoIdAsync(int id)
        {
            var kyNang = await _kyNangRepository.GetByIdAsync(id);
            if (kyNang == null) return null;

            return new KyNangDto
            {
                Id = kyNang.Id,
                TenKyNang = kyNang.TenKyNang,
                MoTa = kyNang.MoTa
            };
        }

        public async Task<KyNangDto> TaoKyNangAsync(TaoKyNangDto dto)
        {
            var kyNang = new KyNang
            {
                TenKyNang = dto.TenKyNang,
                MoTa = dto.MoTa
            };

            await _kyNangRepository.AddAsync(kyNang);
            await _kyNangRepository.SaveChangesAsync();

            return new KyNangDto
            {
                Id = kyNang.Id,
                TenKyNang = kyNang.TenKyNang,
                MoTa = kyNang.MoTa
            };
        }

        public async Task<bool> CapNhatAsync(int id, CapNhatKyNangDto dto)
        {
            var kyNang = await _kyNangRepository.GetByIdAsync(id);
            if (kyNang == null) return false;

            kyNang.TenKyNang = dto.TenKyNang;
            kyNang.MoTa = dto.MoTa;

            await _kyNangRepository.UpdateAsync(kyNang);
            await _kyNangRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> XoaAsync(int id)
        {
            var kyNang = await _kyNangRepository.GetByIdAsync(id);
            if (kyNang == null) return false;

            await _kyNangRepository.DeleteAsync(kyNang);
            await _kyNangRepository.SaveChangesAsync();

            return true;
        }
    }
}
