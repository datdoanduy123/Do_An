using Apllication.DTOs;
using Apllication.IRepositories;
using Apllication.IService;

namespace Apllication.Service
{
    // Trien khai Service cho thuc the NhomQuyen
    public class NhomQuyenService : INhomQuyenService
    {
        private readonly INhomQuyenRepository _nhomQuyenRepo;

        public NhomQuyenService(INhomQuyenRepository nhomQuyenRepo)
        {
            _nhomQuyenRepo = nhomQuyenRepo;
        }

        public async Task<NhomQuyenDto> TaoNhomQuyenAsync(TaoNhomQuyenDto taoNhomQuyenDto)
        {
            // Validate: Kiem tra Ten nhom da ton tai chua
            var tonTai = await _nhomQuyenRepo.KiemTraTenNhomTonTaiAsync(taoNhomQuyenDto.TenNhom);
            if (tonTai)
            {
                throw new Exception("Ten nhom quyen da ton tai tren he thong.");
            }

            return await _nhomQuyenRepo.TaoNhomQuyenAsync(taoNhomQuyenDto);
        }

        public async Task<KetQuaPhanTrangDto<NhomQuyenDto>> LayDanhSachNhomQuyenAsync(NhomQuyenQueryDto query)
        {
            return await _nhomQuyenRepo.LayDanhSachNhomQuyenAsync(query);
        }

        public async Task<NhomQuyenDto?> LayTheoIdAsync(int id)
        {
            return await _nhomQuyenRepo.LayTheoIdAsync(id);
        }

        public async Task<bool> CapNhatAsync(int id, CapNhatNhomQuyenDto dto)
        {
            return await _nhomQuyenRepo.CapNhatAsync(id, dto);
        }

        public async Task<bool> XoaAsync(int id)
        {
            return await _nhomQuyenRepo.XoaAsync(id);
        }
    }
}
