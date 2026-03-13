using Domain.Enums;
using System;

namespace Apllication.DTOs.Sprint
{
    public class SprintDto
    {
        public int Id { get; set; }
        public int DuAnId { get; set; }
        public string TenSprint { get; set; } = string.Empty;
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public int MucTieuStoryPoints { get; set; }
        public TrangThaiSprint TrangThai { get; set; }
    }

    public class TaoSprintDto
    {
        public int DuAnId { get; set; }
        public string TenSprint { get; set; } = string.Empty;
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public int MucTieuStoryPoints { get; set; }
    }

    public class CapNhatSprintDto
    {
        public string TenSprint { get; set; } = string.Empty;
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public int MucTieuStoryPoints { get; set; }
        public TrangThaiSprint TrangThai { get; set; }
    }
}
