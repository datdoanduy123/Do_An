namespace Domain.Entities
{
    // Thuc the Vai tro
    public class VaiTro
    {
        public int Id { get; set; }
        public string TenVaiTro { get; set; } = string.Empty;
        public string MaVaiTro { get; set; } = string.Empty;
        public string MoTa { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<NguoiDungVaiTro> NguoiDungVaiTros { get; set; } = new List<NguoiDungVaiTro>();
        public ICollection<VaiTroQuyen> VaiTroQuyens { get; set; } = new List<VaiTroQuyen>();
    }
}
