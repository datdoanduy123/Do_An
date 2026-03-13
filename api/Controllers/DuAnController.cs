using Apllication.DTOs.DuAn;
using Apllication.IService;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DuAnController : BaseController
    {
        private readonly IDuAnService _duAnService;

        public DuAnController(IDuAnService duAnService)
        {
            _duAnService = duAnService;
        }

        [HttpGet("danh-sach")]
        public async Task<IActionResult> LayDanhSach()
        {
            try
            {
                var result = await _duAnService.GetAllAsync();
                return SuccessResponse(result, "Lay danh sach du an thanh cong.");
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
                var result = await _duAnService.GetByIdAsync(id);
                if (result == null) return ErrorResponse(404, "Khong tim thay du an.");
                return SuccessResponse(result, "Lay thong tin du an thanh cong.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [HttpPost("tao-du-an")]
        public async Task<IActionResult> TaoDuAn([FromBody] TaoDuAnDto dto)
        {
            try
            {
                var result = await _duAnService.CreateAsync(dto);
                return SuccessResponse(result, "Tao du an thanh cong.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> CapNhat(int id, [FromBody] CapNhatDuAnDto dto)
        {
            try
            {
                var result = await _duAnService.UpdateAsync(id, dto);
                if (result) return SuccessResponse(null!, "Cap nhat du an thanh cong.");
                return ErrorResponse(400, "Khong the cap nhat du an.");
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
                var result = await _duAnService.DeleteAsync(id);
                if (result) return SuccessResponse(null!, "Xoa du an thanh cong.");
                return ErrorResponse(400, "Khong the xoa du an.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }
    }
}
