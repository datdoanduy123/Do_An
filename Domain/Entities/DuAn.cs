using Domain.Enums;
using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    /// <summary>
    /// Thực thể lưu trữ thông tin về Dự án.
    /// Quản lý vòng đời của một dự án phần mềm từ lúc khởi tạo đến khi kết thúc.
    /// </summary>
    public class DuAn
    {
        public int Id { get; set; }

        public string TenDuAn { get; set; } = string.Empty;

        public string? MoTa { get; set; }

        public DateTime NgayBatDau { get; set; }

        public DateTime? NgayKetThuc { get; set; }

        /// <summary>
        /// Trạng thái dự án (Ví dụ: Planning, Active, Completed, Cancelled)
        /// </summary>
        public TrangThaiDuAn TrangThai { get; set; } = TrangThaiDuAn.Planning;
        public int? CreatedBy { get; set; }
        public User? Creator { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Quan hệ: Một dự án có nhiều tài liệu đi kèm
        public ICollection<TaiLieuDuAn> TaiLieuDuAns { get; set; } = new List<TaiLieuDuAn>();

        // Quan hệ: Một dự án có nhiều Sprint
        public ICollection<Sprint> Sprints { get; set; } = new List<Sprint>();

        // Quan hệ: Một dự án có nhiều công việc (nằm trong Backlog hoặc Sprint)
        public ICollection<CongViec> CongViecs { get; set; } = new List<CongViec>();
    }
}
