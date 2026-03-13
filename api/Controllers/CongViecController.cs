using Apllication.DTOs.CongViec;
using Apllication.IService;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CongViecController : BaseController
    {
        private readonly ICongViecService _congViecService;

        public CongViecController(ICongViecService congViecService)
        {
            _congViecService = congViecService;
        }

        [HttpGet("du-an/{projectId}")]
        public async Task<IActionResult> LayTheoDuAn(int projectId)
        {
            try
            {
                var result = await _congViecService.GetByProjectIdAsync(projectId);
                return SuccessResponse(result, "Lay danh sach cong viec theo du an thanh cong.");
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
                var result = await _congViecService.GetByIdAsync(id);
                if (result == null) return ErrorResponse(404, "Khong tim thay cong viec.");
                return SuccessResponse(result, "Lay thong tin cong viec thanh cong.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [HttpPost("tao-cong-viec")]
        public async Task<IActionResult> TaoCongViec([FromBody] TaoCongViecDto dto)
        {
            try
            {
                var result = await _congViecService.CreateAsync(dto);
                return SuccessResponse(result, "Tao cong viec thanh cong.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [HttpPatch("{id}/cap-nhat-trang-thai")]
        public async Task<IActionResult> CapNhatTrangThai(int id, [FromQuery] string status)
        {
            try
            {
                var result = await _congViecService.UpdateStatusAsync(id, status);
                if (result) return SuccessResponse(null!, "Cap nhat trang thai thanh cong.");
                return ErrorResponse(400, "Khong the cap nhat trang thai.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        /// <summary>
        /// API thực hiện giao việc thủ công cho nhân viên.
        /// </summary>
        [HttpPost("giao-viec-thu-cong")]
        public async Task<IActionResult> GiaoViecThuCong([FromBody] GiaoViecThuCongDto dto)
        {
            try
            {
                var result = await _congViecService.GiaoViecThuCongAsync(dto);
                if (result) return SuccessResponse(null!, "Giao viec thu cong thanh cong.");
                return ErrorResponse(400, "Khong the giao viec cho nhan vien nay.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }
    }
}
