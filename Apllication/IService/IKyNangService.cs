using Apllication.DTOs;

namespace Apllication.IService
{
    public interface IKyNangService
    {
        Task<KetQuaPhanTrangDto<KyNangDto>> LayDanhSachKyNangAsync(KyNangQueryDto query);
        Task<KyNangDto?> LayTheoIdAsync(int id);
        Task<KyNangDto> TaoKyNangAsync(TaoKyNangDto dto);
        Task<bool> CapNhatAsync(int id, CapNhatKyNangDto dto);
        Task<bool> XoaAsync(int id);

        // Các phương thức phân cấp mới
        Task<IEnumerable<NhomKyNangDto>> GetHierarchyAsync();
        Task<IEnumerable<NhomKyNangDto>> GetAllNhomAsync();
        Task<IEnumerable<CongNgheDto>> GetCongNgheByNhomAsync(int nhomId);
        Task<NhomKyNangDto> TaoNhomAsync(TaoNhomKyNangDto dto);
        Task<CongNgheDto> TaoCongNgheAsync(TaoCongNgheDto dto);
    }
}
