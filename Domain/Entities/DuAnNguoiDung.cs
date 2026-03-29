using System;
using Domain.Enums;

namespace Domain.Entities
{
    /// <summary>
    /// Thực thể liên kết Người dùng và Dự án (Project Membership).
    /// Cho phép phân quyền cụ thể cho từng thành viên trong một dự án.
    /// </summary>
    public class DuAnNguoiDung
    {
        public int Id { get; set; }
        public int DuAnId { get; set; }
        public DuAn DuAn { get; set; } = null!;
        public int NguoiDungId { get; set; }
        public User NguoiDung { get; set; } = null!;

        /// <summary>
        /// Vai trò trong dự án (ví dụ: PM, Developer, Tester, Client)
        /// </summary>
        public ProjectRole ProjectRole { get; set; } = ProjectRole.Member;

        public DateTime JointAt { get; set; } = DateTime.UtcNow;
    }
}
