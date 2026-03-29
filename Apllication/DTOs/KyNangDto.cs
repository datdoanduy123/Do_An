namespace Apllication.DTOs
{
    public class KyNangDto
    {
        public int Id { get; set; }
        public string TenKyNang { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public int CongNgheId { get; set; }
        public string? TenCongNghe { get; set; }
        public string? TenNhomKyNang { get; set; }
    }
}
