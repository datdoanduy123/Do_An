using System;

namespace Domain.Entities
{
    /// <summary>
    /// Thực thể quản lý sự phụ thuộc giữa các công việc (Finish-to-Start).
    /// Ví dụ: Task B chỉ có thể bắt đầu sau khi Task A hoàn thành.
    /// </summary>
    public class PhuThuocCongViec
    {
        public int Id { get; set; }

        /// <summary>
        /// ID của công việc bị phụ thuộc (Task B).
        /// </summary>
        public int TaskId { get; set; }
        public CongViec? Task { get; set; }

        /// <summary>
        /// ID của công việc tiền đề (Task A - phải xong trước).
        /// </summary>
        public int DependsOnTaskId { get; set; }
        public CongViec? DependsOnTask { get; set; }

        /// <summary>
        /// Kiểu phụ thuộc (Mặc định: Finish-to-Start).
        /// </summary>
        public string DependencyType { get; set; } = "FS"; 
    }
}
