namespace Apllication.IService
{
    // Giao dien dich vu ma hoa va xac minh mat khau
    public interface IMatKhauService
    {
        string TaoPasswordHash(string matKhau);
        bool XacMinhPassword(string matKhau, string hashDaLuu);
    }
}
