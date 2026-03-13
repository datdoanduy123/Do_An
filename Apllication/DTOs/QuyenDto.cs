namespace Apllication.DTOs
{
    // DTO dung de nhan thong tin khi tao quyen moi
    public class TaoQuyenDto
    {
        public string TenQuyen { get; set; } = string.Empty;
        public string MaQuyen { get; set; } = string.Empty; // Vi du: USER_CREATE
        public string MoTa { get; set; } = string.Empty;
        public int NhomQuyenId { get; set; }
    }

    // DTO dung de tra ve thong tin quyen sau khi tao
    public class QuyenDto
    {
        public int Id { get; set; }
        public string TenQuyen { get; set; } = string.Empty;
        public string MaQuyen { get; set; } = string.Empty;
        public string MoTa { get; set; } = string.Empty;
        public int NhomQuyenId { get; set; }
        public string TenNhomQuyen { get; set; } = string.Empty;
    }
}
