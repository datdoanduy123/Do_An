using System;

namespace Domain.Entities
{
    /// <summary>
    /// Thực thể lưu trữ thông tin về tài liệu dự án do người dùng tải lên.
    /// Đây là file nguồn để AI phân tích và sinh ra các công việc (Tasks).
    /// </summary>
    public class TaiLieuDuAn
    {
        public int Id { get; set; }

        public int DuAnId { get; set; }
        public DuAn? DuAn { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public string FileType { get; set; } = string.Empty; // .docx, .xlsx, .pdf

        /// <summary>
        /// ID của người dùng thực hiện tải tệp lên.
        /// </summary>
        public int UploadedBy { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Đánh dấu tài liệu này đã được AI xử lý để sinh việc hay chưa.
        /// </summary>
        public bool IsProcessed { get; set; } = false;
    }
}
