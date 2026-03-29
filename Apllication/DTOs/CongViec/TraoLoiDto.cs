using System;

namespace Apllication.DTOs.CongViec
{
    public class TraoLoiDto
    {
        public int Id { get; set; }
        public int NguoiTaoId { get; set; }
        public string? TenNguoiTao { get; set; }
        public string NoiDung { get; set; } = string.Empty;
        public int Loai { get; set; } // 0: Thảo luận, 1: Lý do từ chối
        public DateTime CreatedAt { get; set; }
    }

    public class TaoTraoLoiDto
    {
        public string NoiDung { get; set; } = string.Empty;
        public int Loai { get; set; } = 0;
    }
}
