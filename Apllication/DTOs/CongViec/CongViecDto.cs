using System;

namespace Apllication.DTOs.CongViec
{
    public class CongViecDto
    {
        public int Id { get; set; }
        public int DuAnId { get; set; }
        public int? SprintId { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public string LoaiCongViec { get; set; } = string.Empty;
        public string DoUuTien { get; set; } = string.Empty;
        public string TrangThai { get; set; } = string.Empty;
        public int StoryPoints { get; set; }
        public int? AssigneeId { get; set; }
        public string? AssigneeName { get; set; }
        public string PhuongThucGiaoViec { get; set; } = string.Empty;
        public double ThoiGianUocTinh { get; set; }
        public double? ThoiGianThucTe { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
    }

    public class TaoCongViecDto
    {
        public int DuAnId { get; set; }
        public int? SprintId { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public string LoaiCongViec { get; set; } = string.Empty;
        public string DoUuTien { get; set; } = "Medium";
        public int StoryPoints { get; set; }
        public double ThoiGianUocTinh { get; set; }
    }

    public class GiaoViecThuCongDto
    {
        public int CongViecId { get; set; }
        public int AssigneeId { get; set; }
    }
}
