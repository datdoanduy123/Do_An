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

        /// <summary>
        /// Thứ tự sắp xếp của Task trong danh sách (giúp PM điều chỉnh thủ công).
        /// </summary>
        public int ViTri { get; set; }

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
        /// Lịch trình Dự kiến (do AI hoặc PM lập kế hoạch).
        /// </summary>
        public DateTime? NgayBatDauDuKien { get; set; }
        public DateTime? NgayKetThucDuKien { get; set; }

        /// <summary>
        /// Lịch trình Thực tế (ghi nhận khi User thay đổi trạng thái).
        /// </summary>
        public DateTime? NgayBatDauThucTe { get; set; }
        public DateTime? NgayKetThucThucTe { get; set; }

        /// <summary>
        /// Số lần công việc bị từ chối (trả về từ trạng thái Review).
        /// </summary>
        public int SoLanBiTuChoi { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? CreatedBy { get; set; }
        public User? Creator { get; set; }

        // Quan hệ: Một công việc có thể đòi hỏi nhiều kỹ năng
        public ICollection<YeuCauCongViec> YeuCauCongViecs { get; set; } = new List<YeuCauCongViec>();

        // Quan hệ: Một công việc có thể phụ thuộc vào nhiều công việc khác
        public ICollection<PhuThuocCongViec> Dependencies { get; set; } = new List<PhuThuocCongViec>();

        // Quan hệ: Một công việc có thể có nhiều trao đổi, thảo luận
        public ICollection<TraoLoiCongViec> TraoLoiCongViecs { get; set; } = new List<TraoLoiCongViec>();

        // Quan hệ: Một công việc có nhiều nhật ký ghi nhận tiến độ
        public ICollection<NhatKyCongViec> NhatKyCongViecs { get; set; } = new List<NhatKyCongViec>();
    }
}
