namespace Apllication.DTOs
{
    // Lop chua thong tin ket qua dang nhap (User info + JWT Token)
    public class DangNhapKetQuaDto
    {
        public NguoiDungDto NguoiDung { get; set; } = null!;
        public string Token { get; set; } = string.Empty;
    }
}
