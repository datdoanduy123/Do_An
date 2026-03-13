namespace Domain.Entities
{
    // Bang trung gian User - VaiTro
    public class NguoiDungVaiTro
    {
        public int NguoiDungId { get; set; }
        public User NguoiDung { get; set; } = null!;

        public int VaiTroId { get; set; }
        public VaiTro VaiTro { get; set; } = null!;
    }
}
