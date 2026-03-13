using System;

namespace Apllication.DTOs.NhatKyCongViec
{
    public class NhatKyDto
    {
        public int Id { get; set; }
        public int CongViecId { get; set; }
        public string? TieuDeCongViec { get; set; }
        public int NguoiCapNhatId { get; set; }
        public string? TenNguoiCapNhat { get; set; }
        public double SoGioLamViec { get; set; }
        public string? GhiChu { get; set; }
        public DateTime NgayCapNhat { get; set; }
    }

    public class TaoNhatKyDto
    {
        public int CongViecId { get; set; }
        public double SoGioLamViec { get; set; }
        public string? GhiChu { get; set; }
    }
}
