using Microsoft.AspNetCore.Authorization;
using api.Attributes;
using Apllication.DTOs;
using Apllication.IService;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NguoiDungController : BaseController
    {
        private readonly INguoiDungService _nguoiDungService;

        public NguoiDungController(INguoiDungService nguoiDungService)
        {
            _nguoiDungService = nguoiDungService;
        }

        [QuyenHan("USER_CREATE")]
        [HttpPost("tao-nguoi-dung")]
        public async Task<IActionResult> TaoNguoiDung(TaoNguoiDungDto taoNguoiDungDto)
        {
            try
            {
                var user = await _nguoiDungService.TaoNguoiDungAsync(taoNguoiDungDto);
                return SuccessResponse(user, "Tao nguoi dung thanh cong.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [QuyenHan("USER_VIEW")]
        [HttpGet("danh-sach")]
        public async Task<IActionResult> LayDanhSach([FromQuery] NguoiDungQueryDto query)
        {
            try
            {
                var result = await _nguoiDungService.LayDanhSachNguoiDungAsync(query);
                return SuccessResponse(result, "Lay danh sach nguoi dung thanh cong.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [HttpGet("profile")]
        public async Task<IActionResult> Profile()
        {
            try
            {
                var result = await _nguoiDungService.LayTheoIdAsync(CurrentUserId);
                if (result == null) return ErrorResponse(404, "Khong tim thay thong tin ca nhan.");
                return SuccessResponse(result, "Lay thong tin ca nhan thanh cong.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [QuyenHan("USER_VIEW")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> ChiTiet(int id)
        {
            try
            {
                var result = await _nguoiDungService.LayTheoIdAsync(id);
                if (result == null) return ErrorResponse(404, "Khong tim thay nguoi dung.");
                return SuccessResponse(result, "Lay thong tin chi tiet thanh cong.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [QuyenHan("USER_UPDATE")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> CapNhat(int id, [FromBody] CapNhatNguoiDungDto dto)
        {
            try
            {
                var result = await _nguoiDungService.CapNhatAsync(id, dto);
                if (result) return SuccessResponse(null!, "Cap nhat thong tin thanh cong.");
                return ErrorResponse(400, "Khong the cap nhat thong tin.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [QuyenHan("USER_DELETE")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Xoa(int id)
        {
            try
            {
                var result = await _nguoiDungService.XoaMemAsync(id);
                if (result) return SuccessResponse(null!, "Xoa nguoi dung thanh cong (Xoa mem).");
                return ErrorResponse(400, "Khong the xoa nguoi dung.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [QuyenHan("USER_UPDATE")]
        [HttpPost("gan-kynang")]
        public async Task<IActionResult> GanKyNang([FromBody] GanKyNangDto dto)
        {
            try
            {
                var result = await _nguoiDungService.GanKyNangAsync(dto);
                if (result) return SuccessResponse(null!, "Gan ky nang thanh cong.");
                return ErrorResponse(400, "Khong the gan ky nang.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [QuyenHan("USER_UPDATE")]
        [HttpPost("go-kynang")]
        public async Task<IActionResult> GoKyNang([FromBody] GanKyNangDto dto)
        {
            try
            {
                var result = await _nguoiDungService.GoKyNangAsync(dto);
                if (result) return SuccessResponse(null!, "Go ky nang thanh cong.");
                return ErrorResponse(400, "Khong the go ky nang.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }
    }
}
