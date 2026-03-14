namespace Apllication.DTOs
{
    public class UserSkillDto
    {
        public int KyNangId { get; set; }
        public string TenKyNang { get; set; } = string.Empty;
        public int Level { get; set; }
        public int SoNamKinhNghiem { get; set; }
    }

    public class GanKyNangDto 
    {
        public int NguoiDungId { get; set; }
        public int KyNangId { get; set; }
        public int Level { get; set; }
        public int SoNamKinhNghiem { get; set; }
    }
}
