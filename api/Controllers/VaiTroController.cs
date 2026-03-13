using Apllication.DTOs;
using Apllication.IService;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VaiTroController : BaseController
    {
        private readonly IVaiTroService _vaiTroService;

        public VaiTroController(IVaiTroService vaiTroService)
        {
            _vaiTroService = vaiTroService;
        }

        [HttpPost("tao-vaitro")]
        public async Task<IActionResult> TaoVaiTro([FromBody] TaoVaiTroDto taoVaiTroDto)
        {
            try
            {
                var result = await _vaiTroService.TaoVaiTroAsync(taoVaiTroDto);
                return SuccessResponse(result, "Tao vai tro thanh cong.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(400, ex.Message);
            }
        }

        [HttpPost("gan-vaitro")]
        public async Task<IActionResult> GanVaiTro([FromBody] GanVaiTroDto ganVaiTroDto)
        {
            try
            {
                var result = await _vaiTroService.GanVaiTroChoNguoiDungAsync(ganVaiTroDto);
                if (result) return SuccessResponse(null!, "Gan vai tro thanh cong.");
                return ErrorResponse(400, "Khong the gan vai tro.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [HttpPost("gan-quyen-cho-vaitro")]
        public async Task<IActionResult> GanQuyenChoVaiTro([FromBody] GanQuyenChoVaiTroDto ganQuyenDto)
        {
            try
            {
                var result = await _vaiTroService.GanQuyenChoVaiTroAsync(ganQuyenDto);
                if (result) return SuccessResponse(null!, "Gan quyen cho vai tro thanh cong.");
                return ErrorResponse(400, "Khong the gan quyen cho vai tro.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [HttpGet("danh-sach")]
        public async Task<IActionResult> LayDanhSach([FromQuery] VaiTroQueryDto query)
        {
            try
            {
                var result = await _vaiTroService.LayDanhSachVaiTroAsync(query);
                return SuccessResponse(result, "Lay danh sach vai tro thanh cong.");
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
                var result = await _vaiTroService.LayTheoIdAsync(id);
                if (result == null) return ErrorResponse(404, "Khong tim thay vai tro.");
                return SuccessResponse(result, "Lay thong tin chi tiet thanh cong.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> CapNhat(int id, [FromBody] CapNhatVaiTroDto dto)
        {
            try
            {
                var result = await _vaiTroService.CapNhatAsync(id, dto);
                if (result) return SuccessResponse(null!, "Cap nhat vai tro thanh cong.");
                return ErrorResponse(400, "Khong the cap nhat vai tro.");
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
                var result = await _vaiTroService.XoaAsync(id);
                if (result) return SuccessResponse(null!, "Xoa vai tro thanh cong.");
                return ErrorResponse(400, "Khong the xoa vai tro.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [HttpPost("go-vaitro")]
        public async Task<IActionResult> GoVaiTro([FromBody] GanVaiTroDto dto)
        {
            try
            {
                var result = await _vaiTroService.GoVaiTroKhoiNguoiDungAsync(dto);
                if (result) return SuccessResponse(null!, "Go vai tro thanh cong.");
                return ErrorResponse(400, "Khong the go vai tro.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [HttpPost("go-quyen")]
        public async Task<IActionResult> GoQuyen([FromBody] GanQuyenChoVaiTroDto dto)
        {
            try
            {
                var result = await _vaiTroService.GoQuyenKhoiVaiTroAsync(dto);
                if (result) return SuccessResponse(null!, "Go quyen thanh cong.");
                return ErrorResponse(400, "Khong the go quyen.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [HttpGet("{id}/quyens")]
        public async Task<IActionResult> LayDanhSachQuyen(int id)
        {
            try
            {
                var result = await _vaiTroService.LayDanhSachQuyenTheoVaiTroAsync(id);
                return SuccessResponse(result, "Lay danh sach quyen cua vai tro thanh cong.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }
    }
}
