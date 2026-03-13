using Apllication.DTOs;
using Apllication.IService;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaiKhoanController : BaseController
    {
        private readonly ITaiKhoanService _taiKhoanService;

        public TaiKhoanController(ITaiKhoanService taiKhoanService)
        {
            _taiKhoanService = taiKhoanService;
        }

        [HttpPost("dang-nhap")]
        public async Task<IActionResult> DangNhap(DangNhapDto dangNhapDto)
        {
            try
            {
                var ketQua = await _taiKhoanService.DangNhapAsync(dangNhapDto);

                if (ketQua == null)
                {
                    return ErrorResponse(401, "Ten dang nhap hoac mat khau khong dung.");
                }

                return SuccessResponse(ketQua, "Dang nhap thanh cong.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }
    }
}
