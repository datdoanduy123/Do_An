using Apllication.DTOs.CongViec;
using Apllication.IService;
using Domain.Enums;
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
        private readonly IGiaoViecAIService _aiService;

        public CongViecController(ICongViecService congViecService, IGiaoViecAIService aiService)
        {
            _congViecService = congViecService;
            _aiService = aiService;
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

        [HttpGet("my-tasks")]
        public async Task<IActionResult> LayCongViecCuaToi()
        {
            try
            {
                var result = await _congViecService.GetMyTasksAsync(CurrentUserId);
                return SuccessResponse(result, "Lay danh sach cong viec cua toi thanh cong.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [HttpGet("pending-reviews")]
        public async Task<IActionResult> LayCongViecChoDuyet()
        {
            try
            {
                var result = await _congViecService.GetTasksPendingReviewAsync();
                return SuccessResponse(result, "Lay danh sach cong viec cho duyet thanh cong.");
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
                var result = await _congViecService.CreateAsync(dto, CurrentUserId);
                return SuccessResponse(result, "Tao cong viec thanh cong.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> CapNhat(int id, [FromBody] CapNhatCongViecDto dto)
        {
            try
            {
                var result = await _congViecService.UpdateAsync(id, dto);
                return SuccessResponse(result, "Cập nhật công việc thành công.");
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
                var result = await _congViecService.DeleteAsync(id);
                if (result) return SuccessResponse(null!, "Xóa công việc thành công.");
                return ErrorResponse(400, "Không thể xóa công việc.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [HttpPatch("{id}/cap-nhat-trang-thai")]
        public async Task<IActionResult> CapNhatTrangThai(int id, [FromQuery] TrangThaiCongViec status)
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

        [HttpPut("{id}/cap-nhat-tien-do")]
        public async Task<IActionResult> CapNhatTienDo(int id, [FromBody] CapNhatTienDoDto dto)
        {
            try
            {
                var result = await _congViecService.CapNhatTienDoAsync(id, dto, CurrentUserId);
                if (result) return SuccessResponse(null!, "Cap nhat tien do thanh cong.");
                return ErrorResponse(400, "Khong the cap nhat tien do.");
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
                var result = await _congViecService.GiaoViecThuCongAsync(dto, CurrentUserId);
                if (result) return SuccessResponse(null!, "Giao viec thu cong thanh cong.");
                return ErrorResponse(400, "Khong the giao viec cho nhan vien nay.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [HttpGet("{id}/goi-y-ai")]
        public async Task<IActionResult> GetGoiYAI(int id)
        {
            try
            {
                var result = await _aiService.GoiYAssigneeAsync(id);
                return SuccessResponse(result, "Lay goi y AI thanh cong.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [HttpPost("auto-assign-project/{projectId}")]
        public async Task<IActionResult> AutoAssignProject(int projectId)
        {
            try
            {
                var result = await _aiService.TuDongGiaoViecDuAnAsync(projectId);
                return SuccessResponse(result, "Giao viec tu dong cho toan bo du an thanh cong.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }
    }
}
