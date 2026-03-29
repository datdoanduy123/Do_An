using System.Collections.Generic;

namespace Apllication.DTOs
{
    public class NhomKyNangDto
    {
        public int Id { get; set; }
        public string TenNhom { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public List<CongNgheDto> CongNghes { get; set; } = new();
    }

    public class TaoNhomKyNangDto
    {
        public string TenNhom { get; set; } = string.Empty;
        public string? MoTa { get; set; }
    }

    public class CongNgheDto
    {
        public int Id { get; set; }
        public string TenCongNghe { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public int NhomKyNangId { get; set; }
        public string? TenNhom { get; set; }
        public List<KyNangDto> KyNangs { get; set; } = new();
    }

    public class TaoCongNgheDto
    {
        public string TenCongNghe { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public int NhomKyNangId { get; set; }
    }
}
