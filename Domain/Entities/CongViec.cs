using Domain.Enums;
using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    /// <summary>
    /// Thực thể quan trọng nhất lưu trữ thông tin về một đầu việc (Task).
    /// Chứa thông tin về người thực hiện, phương thức giao việc (AI/Thủ công) và giải thích của AI.
    /// </summary>
    public class CongViec
    {
        public int Id { get; set; }

        public int DuAnId { get; set; }
        public DuAn? DuAn { get; set; }

        public int? SprintId { get; set; }
        public Sprint? Sprint { get; set; }

        public string TieuDe { get; set; } = string.Empty;

        public string? MoTa { get; set; }

        /// <summary>
        /// Loại công việc: Frontend, Backend, Tester, DevOps...
        /// </summary>
        public LoaiCongViec LoaiCongViec { get; set; }

        /// <summary>
        /// Độ ưu tiên: Low, Medium, High, Urgent
        /// </summary>
        public DoUuTien DoUuTien { get; set; } = DoUuTien.Medium;

        /// <summary>
        /// Trạng thái: Todo, InProgress, Review, Done
        /// </summary>
        public TrangThaiCongViec TrangThai { get; set; } = TrangThaiCongViec.Todo;

        /// <summary>
        /// Điểm độ khó (Story Points) theo dãy Fibonacci (1, 2, 3, 5, 8, 13...).
        /// </summary>
        public int StoryPoints { get; set; }

        /// <summary>
        /// Người được giao công việc.
        /// </summary>
        public int? AssigneeId { get; set; }
        public User? Assignee { get; set; }

        /// <summary>
        /// Phương thức giao việc: "Manual" (Thủ công) hoặc "AI" (Tự động bởi AI).
        /// </summary>
        public PhuongThucGiaoViec PhuongThucGiaoViec { get; set; } = PhuongThucGiaoViec.Manual;

        /// <summary>
        /// Giải thích của AI tại sao lại chọn người này hoặc tại sao lại đưa vào Sprint này.
        /// </summary>
        public string? AiReasoning { get; set; }

        /// <summary>
        /// Điểm khớp (0-100) mà AI tính toán được giữa Task và User.
        /// </summary>
        public double? AiMatchScore { get; set; }

        /// <summary>
        /// Thời gian ước tính hoàn thành (Giờ).
        /// </summary>
        public double ThoiGianUocTinh { get; set; }

        /// <summary>
        /// Thời gian thực tế đã thực hiện (Giờ).
        /// </summary>
        public double? ThoiGianThucTe { get; set; }

        /// <summary>
        /// Ngày bắt đầu dự kiến hoặc thực tế.
        /// </summary>
        public DateTime? NgayBatDau { get; set; }

        /// <summary>
        /// Ngày kết thúc dự kiến hoặc thực tế.
        /// </summary>
        public DateTime? NgayKetThuc { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Quan hệ: Một công việc có thể đòi hỏi nhiều kỹ năng
        public ICollection<YeuCauCongViec> YeuCauCongViecs { get; set; } = new List<YeuCauCongViec>();
    }
}
