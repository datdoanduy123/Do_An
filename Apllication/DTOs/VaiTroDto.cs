namespace Apllication.DTOs
{
    // DTO dung de nhan thong tin khi tao vai tro moi
    public class TaoVaiTroDto
    {
        public string TenVaiTro { get; set; } = string.Empty;
        public string MaVaiTro { get; set; } = string.Empty;
        public string MoTa { get; set; } = string.Empty;
    }

    // DTO dung de tra ve thong tin vai tro sau khi tao
    public class VaiTroDto
    {
        public int Id { get; set; }
        public string TenVaiTro { get; set; } = string.Empty;
        public string MaVaiTro { get; set; } = string.Empty;
        public string MoTa { get; set; } = string.Empty;
    }
}
