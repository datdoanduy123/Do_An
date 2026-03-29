using System.Collections.Generic;

namespace Domain.Entities
{
    /// <summary>
    /// Thực thể Công nghệ - Cấp 2.
    /// Ví dụ: Java, .NET, NodeJs, React. Thuộc về một Nhóm kỹ năng.
    /// </summary>
    public class CongNghe
    {
        public int Id { get; set; }

        public string TenCongNghe { get; set; } = string.Empty;

        public string? MoTa { get; set; }

        // Khóa ngoại đến Nhóm kỹ năng
        public int NhomKyNangId { get; set; }
        public NhomKyNang NhomKyNang { get; set; } = null!;

        // Mối quan hệ: Một công nghệ chứa nhiều kỹ năng chi tiết
        public ICollection<KyNang> KyNangs { get; set; } = new List<KyNang>();
    }
}
