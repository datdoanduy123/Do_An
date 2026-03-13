namespace Domain.Entities
{
    // Thuc the Nhom quyen
    public class NhomQuyen
    {
        public int Id { get; set; }
        public string TenNhom { get; set; } = string.Empty;
        public string MoTa { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Quyen> Quyens { get; set; } = new List<Quyen>();
    }
}
