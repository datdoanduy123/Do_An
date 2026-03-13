using Domain.Entities;

namespace Apllication.IService
{
    // Giao diện cho dịch vụ tạo Token
    public interface IDichVuToken
    {
        string TaoToken(User nguoiDung, List<string> vaiTros);
    }
}
