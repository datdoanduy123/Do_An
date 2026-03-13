namespace Apllication.DTOs
{
    // DTO chua thong tin truy van danh sach nguoi dung
    public class NguoiDungQueryDto : YeuCauPhanTrangDto
    {
        // Tu khoa tim kiem (Ho ten hoac Ten dang nhap)
        public string? Keyword { get; set; }
    }
}
