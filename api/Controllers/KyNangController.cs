using api.Attributes;
using Apllication.DTOs;
using Apllication.IService;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KyNangController : BaseController
    {
        private readonly IKyNangService _kyNangService;

        public KyNangController(IKyNangService kyNangService)
        {
            _kyNangService = kyNangService;
        }

        [HttpGet("danh-sach")]
        public async Task<IActionResult> LayDanhSach([FromQuery] KyNangQueryDto query)
        {
            try
            {
                var result = await _kyNangService.LayDanhSachKyNangAsync(query);
                return SuccessResponse(result, "Lay danh sach ky nang thanh cong.");
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
                var result = await _kyNangService.LayTheoIdAsync(id);
                if (result == null) return ErrorResponse(404, "Khong tim thay ky nang.");
                return SuccessResponse(result, "Lay thong tin chi tiet thanh cong.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [HttpPost("tao-ky-nang")]
        public async Task<IActionResult> TaoKyNang(TaoKyNangDto dto)
        {
            try
            {
                var result = await _kyNangService.TaoKyNangAsync(dto);
                return SuccessResponse(result, "Tao ky nang thanh cong.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> CapNhat(int id, [FromBody] CapNhatKyNangDto dto)
        {
            try
            {
                var result = await _kyNangService.CapNhatAsync(id, dto);
                if (result) return SuccessResponse(null!, "Cap nhat ky nang thanh cong.");
                return ErrorResponse(400, "Khong the cap nhat ky nang.");
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
                var result = await _kyNangService.XoaAsync(id);
                if (result) return SuccessResponse(null!, "Xoa ky nang thanh cong.");
                return ErrorResponse(400, "Khong the xoa ky nang.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }
    }
}
