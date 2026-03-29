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
                MoTa = kyNang.MoTa,
                CongNgheId = kyNang.CongNgheId,
                TenCongNghe = kyNang.CongNghe?.TenCongNghe,
                TenNhomKyNang = kyNang.CongNghe?.NhomKyNang?.TenNhom
            };
        }

        public async Task<KyNangDto> TaoKyNangAsync(TaoKyNangDto dto)
        {
            var kyNang = new KyNang
            {
                TenKyNang = dto.TenKyNang,
                MoTa = dto.MoTa,
                CongNgheId = dto.CongNgheId
            };

            await _kyNangRepository.AddAsync(kyNang);
            await _kyNangRepository.SaveChangesAsync();

            return await LayTheoIdAsync(kyNang.Id) ?? new KyNangDto { Id = kyNang.Id, TenKyNang = kyNang.TenKyNang };
        }

        public async Task<bool> CapNhatAsync(int id, CapNhatKyNangDto dto)
        {
            var kyNang = await _kyNangRepository.GetByIdAsync(id);
            if (kyNang == null) return false;

            kyNang.TenKyNang = dto.TenKyNang;
            kyNang.MoTa = dto.MoTa;
            kyNang.CongNgheId = dto.CongNgheId;

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

        public async Task<IEnumerable<NhomKyNangDto>> GetHierarchyAsync()
        {
            var hierarchy = await _kyNangRepository.GetHierarchyAsync();
            return hierarchy.Select(n => new NhomKyNangDto
            {
                Id = n.Id,
                TenNhom = n.TenNhom,
                MoTa = n.MoTa,
                CongNghes = n.CongNghes.Select(c => new CongNgheDto
                {
                    Id = c.Id,
                    TenCongNghe = c.TenCongNghe,
                    MoTa = c.MoTa,
                    NhomKyNangId = c.NhomKyNangId,
                    KyNangs = c.KyNangs.Select(k => new KyNangDto
                    {
                        Id = k.Id,
                        TenKyNang = k.TenKyNang,
                        MoTa = k.MoTa,
                        CongNgheId = k.CongNgheId
                    }).ToList()
                }).ToList()
            });
        }

        public async Task<IEnumerable<NhomKyNangDto>> GetAllNhomAsync()
        {
            var nhoms = await _kyNangRepository.GetAllNhomKyNangAsync();
            return nhoms.Select(n => new NhomKyNangDto { Id = n.Id, TenNhom = n.TenNhom, MoTa = n.MoTa });
        }

        public async Task<IEnumerable<CongNgheDto>> GetCongNgheByNhomAsync(int nhomId)
        {
            var cns = await _kyNangRepository.GetCongNgheByNhomAsync(nhomId);
            return cns.Select(c => new CongNgheDto { Id = c.Id, TenCongNghe = c.TenCongNghe, MoTa = c.MoTa, NhomKyNangId = c.NhomKyNangId });
        }

        public async Task<NhomKyNangDto> TaoNhomAsync(TaoNhomKyNangDto dto)
        {
            var nhom = new NhomKyNang { TenNhom = dto.TenNhom, MoTa = dto.MoTa };
            await _kyNangRepository.AddNhomAsync(nhom);
            await _kyNangRepository.SaveChangesAsync();
            return new NhomKyNangDto { Id = nhom.Id, TenNhom = nhom.TenNhom, MoTa = nhom.MoTa };
        }

        public async Task<CongNgheDto> TaoCongNgheAsync(TaoCongNgheDto dto)
        {
            var cn = new CongNghe { TenCongNghe = dto.TenCongNghe, MoTa = dto.MoTa, NhomKyNangId = dto.NhomKyNangId };
            await _kyNangRepository.AddCongNgheAsync(cn);
            await _kyNangRepository.SaveChangesAsync();
            return new CongNgheDto { Id = cn.Id, TenCongNghe = cn.TenCongNghe, MoTa = cn.MoTa, NhomKyNangId = cn.NhomKyNangId };
        }
    }
}
