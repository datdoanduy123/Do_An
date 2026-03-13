using Apllication.DTOs;
using Apllication.IRepositories;
using Apllication.IService;

namespace Apllication.Service
{
    // Trien khai Service cho thuc the Quyen
    public class QuyenService : IQuyenService
    {
        private readonly IQuyenRepository _quyenRepo;

        public QuyenService(IQuyenRepository quyenRepo)
        {
            _quyenRepo = quyenRepo;
        }

        public async Task<QuyenDto> TaoQuyenAsync(TaoQuyenDto taoQuyenDto)
        {
            // Validate: Kiem tra Ma quyen da ton tai chua
            var tonTai = await _quyenRepo.KiemTraMaQuyenTonTaiAsync(taoQuyenDto.MaQuyen);
            if (tonTai)
            {
                throw new Exception("Ma quyen da ton tai tren he thong.");
            }

            return await _quyenRepo.TaoQuyenAsync(taoQuyenDto);
        }

        public async Task<KetQuaPhanTrangDto<QuyenDto>> LayDanhSachQuyenAsync(QuyenQueryDto query)
        {
            return await _quyenRepo.LayDanhSachQuyenAsync(query);
        }

        public async Task<QuyenDto?> LayTheoIdAsync(int id)
        {
            return await _quyenRepo.LayTheoIdAsync(id);
        }

        public async Task<bool> CapNhatAsync(int id, CapNhatQuyenDto dto)
        {
            return await _quyenRepo.CapNhatAsync(id, dto);
        }

        public async Task<bool> XoaAsync(int id)
        {
            return await _quyenRepo.XoaAsync(id);
        }
    }
}
