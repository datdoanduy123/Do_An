namespace Apllication.DTOs.QuyTacGiaoViecAI
{
    public class QuyTacGiaoViecAIDto
    {
        public int Id { get; set; }
        public string MaQuyTac { get; set; } = string.Empty;
        public string GiaTri { get; set; } = string.Empty;
        public string LoaiDuLieu { get; set; } = "String";
        public string? MoTa { get; set; }
        public bool IsActive { get; set; }
    }
}
