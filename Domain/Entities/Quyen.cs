namespace Domain.Entities
{
    // Thuc the Quyen chi tiet
    public class Quyen
    {
        public int Id { get; set; }
        public string TenQuyen { get; set; } = string.Empty;
        public string MaQuyen { get; set; } = string.Empty; // Vi du: USER_CREATE
        public string MoTa { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int NhomQuyenId { get; set; }
        public NhomQuyen NhomQuyen { get; set; } = null!;

        public ICollection<VaiTroQuyen> VaiTroQuyens { get; set; } = new List<VaiTroQuyen>();
    }
}
