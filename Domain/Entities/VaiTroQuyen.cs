namespace Domain.Entities
{
    // Bang trung gian VaiTro - Quyen
    public class VaiTroQuyen
    {
        public int VaiTroId { get; set; }
        public VaiTro VaiTro { get; set; } = null!;

        public int QuyenId { get; set; }
        public Quyen Quyen { get; set; } = null!;
    }
}
