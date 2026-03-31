using Domain.Enums;
using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    /// <summary>
    /// Thực thể quản lý định kỳ thời gian trong Agile (Sprint).
    /// Giúp phân nhóm các công việc cần hoàn thành trong một khoảng thời gian nhất định.
    /// </summary>
    public class Sprint
    {
        public int Id { get; set; }

        public int DuAnId { get; set; }
        public DuAn? DuAn { get; set; }

        public string TenSprint { get; set; } = string.Empty;
        public string? MoTa { get; set; }

        public DateTime NgayBatDau { get; set; }

        public DateTime NgayKetThuc { get; set; }

        public TrangThaiSprint TrangThai { get; set; } = TrangThaiSprint.New;
        public int? CreatedBy { get; set; }
        public User? Creator { get; set; }

        // Một Sprint chứa nhiều công việc
        public ICollection<CongViec> CongViecs { get; set; } = new List<CongViec>();
    }
}
