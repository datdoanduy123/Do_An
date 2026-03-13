using Apllication.DTOs;
using Apllication.IService;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuyenController : BaseController
    {
        private readonly IQuyenService _quyenService;

        public QuyenController(IQuyenService quyenService)
        {
            _quyenService = quyenService;
        }

        [HttpPost("tao-quyen")]
        public async Task<IActionResult> TaoQuyen([FromBody] TaoQuyenDto taoQuyenDto)
        {
            try
            {
                var result = await _quyenService.TaoQuyenAsync(taoQuyenDto);
                return SuccessResponse(result, "Tao quyen thanh cong.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(400, ex.Message);
            }
        }

        [HttpGet("danh-sach")]
        public async Task<IActionResult> LayDanhSach([FromQuery] QuyenQueryDto query)
        {
            try
            {
                var result = await _quyenService.LayDanhSachQuyenAsync(query);
                return SuccessResponse(result, "Lay danh sach quyen thanh cong.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ChiTiet(int id)
        {
            try
            {
                var result = await _quyenService.LayTheoIdAsync(id);
                if (result == null) return ErrorResponse(404, "Khong tim thay quyen.");
                return SuccessResponse(result, "Lay thong tin chi tiet thanh cong.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> CapNhat(int id, [FromBody] CapNhatQuyenDto dto)
        {
            try
            {
                var result = await _quyenService.CapNhatAsync(id, dto);
                if (result) return SuccessResponse(null!, "Cap nhat quyen thanh cong.");
                return ErrorResponse(400, "Khong the cap nhat quyen.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Xoa(int id)
        {
            try
            {
                var result = await _quyenService.XoaAsync(id);
                if (result) return SuccessResponse(null!, "Xoa quyen thanh cong.");
                return ErrorResponse(400, "Khong the xoa quyen.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }
    }
}
