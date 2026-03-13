namespace Apllication.DTOs
{
    // Lop chua thong tin chi tiet nguoi dung
    public class NguoiDungDto
    {
        public int Id { get; set; }
        public string TenDangNhap { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DienThoai { get; set; } = string.Empty;
        
        // Danh sach cac vai tro cua nguoi dung
        public List<string> VaiTros { get; set; } = new List<string>();
        
        // Thoi gian tao
        public DateTime CreatedAt { get; set; }
    }
}
