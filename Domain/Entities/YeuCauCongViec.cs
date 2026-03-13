namespace Domain.Entities
{
    /// <summary>
    /// Thực thể định nghĩa yêu cầu kỹ năng cho một công việc cụ thể.
    /// Dùng để AI đối soát xem nhân viên có đủ trình độ làm task này không.
    /// </summary>
    public class YeuCauCongViec
    {
        public int CongViecId { get; set; }
        public CongViec? CongViec { get; set; }

        public int KyNangId { get; set; }
        public KyNang? KyNang { get; set; }

        /// <summary>
        /// Cấp độ tối thiểu yêu cầu (Ví dụ: Cần tối thiểu Level 3 để làm task này).
        /// </summary>
        public int MucDoYeuCau { get; set; }
    }
}
