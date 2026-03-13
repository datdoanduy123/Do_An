using System;

namespace Apllication.DTOs.TaiLieuDuAn
{
    public class TaiLieuDuAnDto
    {
        public int Id { get; set; }
        public int DuAnId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public int UploadedBy { get; set; }
        public DateTime UploadAt { get; set; }
        public bool IsProcessed { get; set; }
    }

    public class UploadTaiLieuDto
    {
        public int DuAnId { get; set; }
        // File thực tế sẽ được xử lý qua IFormFile trong Controller
    }
}
