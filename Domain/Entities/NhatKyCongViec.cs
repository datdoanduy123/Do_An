using System;

namespace Domain.Entities
{
    /// <summary>
    /// Thực thể lưu trữ lịch sử cập nhật tiến độ công việc.
    /// Dùng để vẽ Dashboard, Burndown chart và theo dõi chi tiết quá trình làm việc.
    /// </summary>
    public class NhatKyCongViec
    {
        public int Id { get; set; }
        
        public int CongViecId { get; set; }
        public CongViec? CongViec { get; set; }

        public int NguoiCapNhatId { get; set; }
        public User? NguoiCapNhat { get; set; }

        public double SoGioLamViec { get; set; }
        public string? GhiChu { get; set; }

        public DateTime NgayCapNhat { get; set; } = DateTime.UtcNow;
    }
}
