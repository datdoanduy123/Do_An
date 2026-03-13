using System;

namespace Apllication.DTOs.DuAn
{
    public class DuAnDto
    {
        public int Id { get; set; }
        public string TenDuAn { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public DateTime NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class TaoDuAnDto
    {
        public string TenDuAn { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public DateTime NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
    }

    public class CapNhatDuAnDto
    {
        public string TenDuAn { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public DateTime NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public string TrangThai { get; set; } = string.Empty;
    }
}
