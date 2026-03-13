namespace Domain.Entities
{
    /// <summary>
    /// Thực thể quan trọng để cấu hình AI từ Database.
    /// Chứa các trọng số và tham số logic mà AI sẽ sử dụng để tính toán phân công.
    /// </summary>
    public class QuyTacGiaoViecAI
    {
        public int Id { get; set; }

        /// <summary>
        /// Mã định danh của quy tắc (Ví dụ: MATCH_SKILL_WEIGHT, MAX_WORKLOAD).
        /// </summary>
        public string MaQuyTac { get; set; } = string.Empty;

        /// <summary>
        /// Giá trị của quy tắc (Ví dụ: "0.6", "40", "true").
        /// </summary>
        public string GiaTri { get; set; } = string.Empty;

        /// <summary>
        /// Kiểu dữ liệu để Service có thể ép kiểu (Ví dụ: Double, Integer, Boolean).
        /// </summary>
        public string LoaiDuLieu { get; set; } = "String";

        public string? MoTa { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
