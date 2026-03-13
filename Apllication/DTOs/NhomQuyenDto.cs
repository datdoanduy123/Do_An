namespace Apllication.DTOs
{
    // DTO dung de nhan thong tin khi tao nhom quyen moi
    public class TaoNhomQuyenDto
    {
        public string TenNhom { get; set; } = string.Empty;
        public string MoTa { get; set; } = string.Empty;
    }

    // DTO dung de tra ve thong tin nhom quyen sau khi tao
    public class NhomQuyenDto
    {
        public int Id { get; set; }
        public string TenNhom { get; set; } = string.Empty;
        public string MoTa { get; set; } = string.Empty;
    }
}
