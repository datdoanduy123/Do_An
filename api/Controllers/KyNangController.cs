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
    public class KyNangController : BaseController
    {
        private readonly IKyNangService _kyNangService;

        public KyNangController(IKyNangService kyNangService)
        {
            _kyNangService = kyNangService;
        }

        [QuyenHan("SKILL_VIEW")]
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

        [QuyenHan("SKILL_VIEW")]
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

        [QuyenHan("SKILL_CREATE")]
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

        [QuyenHan("SKILL_UPDATE")]
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

        [QuyenHan("SKILL_DELETE")]
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

        [QuyenHan("SKILL_VIEW")]
        [HttpGet("hierarchy")]
        public async Task<IActionResult> GetHierarchy()
        {
            try
            {
                var result = await _kyNangService.GetHierarchyAsync();
                return SuccessResponse(result, "Lay cau truc phan cap thanh cong.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [QuyenHan("SKILL_VIEW")]
        [HttpGet("nhom-danh-sach")]
        public async Task<IActionResult> GetNhomKyNangs()
        {
            try
            {
                var result = await _kyNangService.GetAllNhomAsync();
                return SuccessResponse(result, "Lay danh sach nhom thanh cong.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [QuyenHan("SKILL_VIEW")]
        [HttpGet("cong-nghe-theo-nhom/{nhomId}")]
        public async Task<IActionResult> GetCongNgheByNhom(int nhomId)
        {
            try
            {
                var result = await _kyNangService.GetCongNgheByNhomAsync(nhomId);
                return SuccessResponse(result, "Lay danh sach cong nghe thanh cong.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [QuyenHan("SKILL_CREATE")]
        [HttpPost("tao-nhom")]
        public async Task<IActionResult> TaoNhom(TaoNhomKyNangDto dto)
        {
            try
            {
                var result = await _kyNangService.TaoNhomAsync(dto);
                return SuccessResponse(result, "Tao nhom ky nang thanh cong.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [QuyenHan("SKILL_CREATE")]
        [HttpPost("tao-cong-nghe")]
        public async Task<IActionResult> TaoCongNghe(TaoCongNgheDto dto)
        {
            try
            {
                var result = await _kyNangService.TaoCongNgheAsync(dto);
                return SuccessResponse(result, "Tao cong nghe thanh cong.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }
    }
}
