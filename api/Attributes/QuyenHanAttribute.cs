using Microsoft.AspNetCore.Authorization;

namespace api.Attributes
{
    // Attribute dung de gan tren Controller/Action de kiem tra quyen
    public class QuyenHanAttribute : AuthorizeAttribute
    {
        public string MaQuyen { get; }

        public QuyenHanAttribute(string maQuyen) : base(policy: maQuyen)
        {
            MaQuyen = maQuyen;
        }
    }
}
