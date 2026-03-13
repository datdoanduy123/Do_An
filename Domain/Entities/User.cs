
namespace Domain.Entities
{
    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
        public string DienThoai { get; set; } = string.Empty;

        public string? VaiTro { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property cho phan quyen
        public ICollection<NguoiDungVaiTro> NguoiDungVaiTros { get; set; } = new List<NguoiDungVaiTro>();

        // Navigation property cho ky nang (Skill matching)
        public ICollection<KyNangNguoiDung> KyNangNguoiDungs { get; set; } = new List<KyNangNguoiDung>();
    }
}
