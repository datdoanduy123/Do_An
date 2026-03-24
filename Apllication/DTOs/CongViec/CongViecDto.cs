using Domain.Enums;
using System;

namespace Apllication.DTOs.CongViec
{
    public class CongViecDto
    {
        public int Id { get; set; }
        public int DuAnId { get; set; }
        public int? SprintId { get; set; }
        public TrangThaiSprint? SprintStatus { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public LoaiCongViec LoaiCongViec { get; set; }
        public DoUuTien DoUuTien { get; set; }
        public TrangThaiCongViec TrangThai { get; set; }
        public int StoryPoints { get; set; }
        public int? AssigneeId { get; set; }
        public string? AssigneeName { get; set; }
        public PhuongThucGiaoViec PhuongThucGiaoViec { get; set; }
        public double ThoiGianUocTinh { get; set; }
        public double? ThoiGianThucTe { get; set; }
        public DateTime? NgayBatDauDuKien { get; set; }
        public DateTime? NgayKetThucDuKien { get; set; }
        public DateTime? NgayBatDauThucTe { get; set; }
        public DateTime? NgayKetThucThucTe { get; set; }
        public DateTime? NgayBatDauSprint { get; set; }
        public DateTime? NgayKetThucSprint { get; set; }
    }

    public class TaoCongViecDto
    {
        public int DuAnId { get; set; }
        public int? SprintId { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public LoaiCongViec LoaiCongViec { get; set; }
        public DoUuTien DoUuTien { get; set; } = DoUuTien.Medium;
        public int StoryPoints { get; set; }
        public double ThoiGianUocTinh { get; set; }
        public DateTime? NgayBatDauDuKien { get; set; }
        public DateTime? NgayKetThucDuKien { get; set; }
    }

    public class GiaoViecThuCongDto
    {
        public int CongViecId { get; set; }
        public int AssigneeId { get; set; }
    }

    public class CapNhatTienDoDto
    {
        public TrangThaiCongViec TrangThai { get; set; }
        public double ThoiGianLamViecThem { get; set; } // Số giờ làm thêm trong lần cập nhật này
        public string? GhiChu { get; set; }
    }
}
