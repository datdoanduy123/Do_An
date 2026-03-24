using Domain.Enums;
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
        public TrangThaiDuAn TrangThai { get; set; }
        public double TienDo { get; set; }
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
        public TrangThaiDuAn TrangThai { get; set; }
    }

    public class ThanhVienDuAnDto
    {
        public int Id { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? VaiTro { get; set; }
        public DateTime NgayThamGia { get; set; }
        public int SoCongViec { get; set; }
        public List<string> KyNang { get; set; } = new();
    }
}
