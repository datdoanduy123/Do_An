using Apllication.DTOs;
using Apllication.IRepositories;
using Apllication.IService;

namespace Apllication.Service
{
    // Trien khai Service cho thuc the VaiTro
    public class VaiTroService : IVaiTroService
    {
        private readonly IVaiTroRepository _vaiTroRepo;

        public VaiTroService(IVaiTroRepository vaiTroRepo)
        {
            _vaiTroRepo = vaiTroRepo;
        }

        public async Task<VaiTroDto> TaoVaiTroAsync(TaoVaiTroDto taoVaiTroDto)
        {
            // Validate: Kiem tra Ten vai tro da ton tai chua
            var tonTai = await _vaiTroRepo.KiemTraTenVaiTroTonTaiAsync(taoVaiTroDto.TenVaiTro);
            if (tonTai)
            {
                throw new Exception("Ten vai tro da ton tai tren he thong.");
            }

            return await _vaiTroRepo.TaoVaiTroAsync(taoVaiTroDto);
        }

        public async Task<bool> GanVaiTroChoNguoiDungAsync(GanVaiTroDto ganVaiTroDto)
        {
            return await _vaiTroRepo.GanVaiTroChoNguoiDungAsync(ganVaiTroDto);
        }

        public async Task<bool> GanQuyenChoVaiTroAsync(GanQuyenChoVaiTroDto ganQuyenDto)
        {
            return await _vaiTroRepo.GanQuyenChoVaiTroAsync(ganQuyenDto);
        }

        public async Task<KetQuaPhanTrangDto<VaiTroDto>> LayDanhSachVaiTroAsync(VaiTroQueryDto query)
        {
            return await _vaiTroRepo.LayDanhSachVaiTroAsync(query);
        }

        public async Task<VaiTroDto?> LayTheoIdAsync(int id)
        {
            return await _vaiTroRepo.LayTheoIdAsync(id);
        }

        public async Task<bool> CapNhatAsync(int id, CapNhatVaiTroDto dto)
        {
            return await _vaiTroRepo.CapNhatAsync(id, dto);
        }

        public async Task<bool> XoaAsync(int id)
        {
            return await _vaiTroRepo.XoaAsync(id);
        }

        public async Task<bool> GoVaiTroKhoiNguoiDungAsync(GanVaiTroDto dto)
        {
            return await _vaiTroRepo.GoVaiTroKhoiNguoiDungAsync(dto);
        }

        public async Task<bool> GoQuyenKhoiVaiTroAsync(GanQuyenChoVaiTroDto dto)
        {
            return await _vaiTroRepo.GoQuyenKhoiVaiTroAsync(dto);
        }

        public async Task<List<QuyenDto>> LayDanhSachQuyenTheoVaiTroAsync(int vaiTroId)
        {
            return await _vaiTroRepo.LayDanhSachQuyenTheoVaiTroAsync(vaiTroId);
        }
    }
}
