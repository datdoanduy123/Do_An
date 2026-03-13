using Apllication.IRepositories;
using Microsoft.AspNetCore.Authorization;

namespace api.Attributes
{
    // Dinh nghia mot Requirement cho Authorization
    public class QuyenHanRequirement : IAuthorizationRequirement
    {
        public string MaQuyen { get; }

        public QuyenHanRequirement(string maQuyen)
        {
            MaQuyen = maQuyen;
        }
    }

    // Xu ly logic kiem tra quyen
    public class QuyenHanHandler : AuthorizationHandler<QuyenHanRequirement>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public QuyenHanHandler(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, QuyenHanRequirement requirement)
        {
            // 1. Kiem tra neu nguoi dung co vai tro QUAN_LY (check tu Claims trong Token) -> Bypass
            if (context.User.IsInRole("QUAN_LY"))
            {
                context.Succeed(requirement);
                return;
            }

            // 2. Neu khong phai QUAN_LY -> Truy van Database de lay danh sach quyen hien tai
            // Vi Handler nay co the la Singleton/Scoped tuy cau hinh, nen dung ScopeFactory de an toan cho Repository
            using var scope = _serviceScopeFactory.CreateScope();
            var nguoiDungRepo = scope.ServiceProvider.GetRequiredService<INguoiDungRepository>();

            var userIdClaim = context.User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.NameId)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return;

            var userId = int.Parse(userIdClaim);
            var quyens = await nguoiDungRepo.LayDanhSachQuyenCuaNguoiDungAsync(userId);

            // 3. Kiem tra MaQuyen yeu cau
            if (quyens.Contains(requirement.MaQuyen))
            {
                context.Succeed(requirement);
            }
        }
    }
}
