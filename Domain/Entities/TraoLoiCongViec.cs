using System;

namespace Domain.Entities
{
    /// <summary>
    /// Thực thể lưu trữ các thảo luận, bình luận hoặc lý do từ chối trên một công việc.
    /// </summary>
    public class TraoLoiCongViec
    {
        public int Id { get; set; }

        public int CongViecId { get; set; }
        public CongViec? CongViec { get; set; }

        public int NguoiTaoId { get; set; }
        public User? NguoiTao { get; set; }

        public string NoiDung { get; set; } = string.Empty;

        /// <summary>
        /// Loại trao đổi: 
        /// 0: Thảo luận bình thường (Normal)
        /// 1: Lý do từ chối (Rejection Reason)
        /// </summary>
        public int Loai { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
