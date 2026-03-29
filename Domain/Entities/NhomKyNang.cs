using System.Collections.Generic;

namespace Domain.Entities
{
    /// <summary>
    /// Thực thể Nhóm kỹ năng (Lĩnh vực) - Cấp 1.
    /// Ví dụ: Backend, Frontend, Mobile, DevOps...
    /// </summary>
    public class NhomKyNang
    {
        public int Id { get; set; }

        public string TenNhom { get; set; } = string.Empty;

        public string? MoTa { get; set; }

        // Mối quan hệ: Một nhóm kỹ năng chứa nhiều công nghệ
        public ICollection<CongNghe> CongNghes { get; set; } = new List<CongNghe>();
    }
}
