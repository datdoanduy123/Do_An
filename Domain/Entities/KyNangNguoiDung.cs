namespace Domain.Entities
{
    /// <summary>
    /// Thực thể liên kết giữa Người dùng và Kỹ năng (Mối quan hệ n-n).
    /// Lưu trữ cấp độ và số năm kinh nghiệm thực tế của mỗi nhân viên.
    /// </summary>
    public class KyNangNguoiDung
    {
        public int UserId { get; set; }
        public User? User { get; set; }

        public int KyNangId { get; set; }
        public KyNang? KyNang { get; set; }

        /// <summary>
        /// Cấp độ kỹ năng (Ví dụ: 1 - Junior, 5 - Expert).
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Số năm kinh nghiệm làm việc với kỹ năng này.
        /// </summary>
        public int SoNamKinhNghiem { get; set; }
    }
}
