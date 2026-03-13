using System.Collections.Generic;

namespace Domain.Entities
{
    /// <summary>
    /// Thực thể danh mục Kỹ năng (Ví dụ: C#, React, SQL).
    /// Dùng để quản lý các kỹ năng cần thiết cho công việc và kỹ năng của nhân viên.
    /// </summary>
    public class KyNang
    {
        public int Id { get; set; }

        public string TenKyNang { get; set; } = string.Empty;

        public string? MoTa { get; set; }

        // Mối quan hệ: Một kỹ năng có thể được sở hữu bởi nhiều nhân viên
        public ICollection<KyNangNguoiDung> KyNangNguoiDungs { get; set; } = new List<KyNangNguoiDung>();

        // Mối quan hệ: Một kỹ năng có thể được yêu cầu bởi nhiều công việc
        public ICollection<YeuCauCongViec> YeuCauCongViecs { get; set; } = new List<YeuCauCongViec>();
    }
}
