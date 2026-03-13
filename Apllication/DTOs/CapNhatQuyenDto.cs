namespace Apllication.DTOs
{
    // DTO dung de cap nhat thong tin quyen
    public class CapNhatQuyenDto
    {
        public string TenQuyen { get; set; } = string.Empty;
        public string MoTa { get; set; } = string.Empty;
        public int NhomQuyenId { get; set; }
    }
}
