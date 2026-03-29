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
    public class NhomQuyenController : BaseController
    {
        private readonly INhomQuyenService _nhomQuyenService;

        public NhomQuyenController(INhomQuyenService nhomQuyenService)
        {
            _nhomQuyenService = nhomQuyenService;
        }

        [QuyenHan("PERMGROUP_CREATE")]
        [HttpPost("tao-nhomquyen")]
        public async Task<IActionResult> TaoNhomQuyen([FromBody] TaoNhomQuyenDto taoNhomDto)
        {
            try
            {
                var result = await _nhomQuyenService.TaoNhomQuyenAsync(taoNhomDto);
                return SuccessResponse(result, "Tao nhom quyen thanh cong.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(400, ex.Message);
            }
        }

        [QuyenHan("PERMGROUP_VIEW")]
        [HttpGet("danh-sach")]
        public async Task<IActionResult> LayDanhSach([FromQuery] NhomQuyenQueryDto query)
        {
            try
            {
                var result = await _nhomQuyenService.LayDanhSachNhomQuyenAsync(query);
                return SuccessResponse(result, "Lay danh sach nhom quyen thanh cong.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [QuyenHan("PERMGROUP_VIEW")]
        [HttpGet("{id}")]
        public async Task<IActionResult> ChiTiet(int id)
        {
            try
            {
                var result = await _nhomQuyenService.LayTheoIdAsync(id);
                if (result == null) return ErrorResponse(404, "Khong tim thay nhom quyen.");
                return SuccessResponse(result, "Lay thong tin chi tiet thanh cong.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [QuyenHan("PERMGROUP_UPDATE")]
        [HttpPut("{id}")]
        public async Task<IActionResult> CapNhat(int id, [FromBody] CapNhatNhomQuyenDto dto)
        {
            try
            {
                var result = await _nhomQuyenService.CapNhatAsync(id, dto);
                if (result) return SuccessResponse(null!, "Cap nhat nhom quyen thanh cong.");
                return ErrorResponse(400, "Khong the cap nhat nhom quyen.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [QuyenHan("PERMGROUP_DELETE")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Xoa(int id)
        {
            try
            {
                var result = await _nhomQuyenService.XoaAsync(id);
                if (result) return SuccessResponse(null!, "Xoa nhom quyen thanh cong.");
                return ErrorResponse(400, "Khong the xoa nhom quyen.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }
    }
}
