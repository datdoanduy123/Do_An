using Microsoft.AspNetCore.Authorization;
using api.Attributes;
using Apllication.DTOs.Sprint;
using Apllication.IService;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SprintController : BaseController
    {
        private readonly ISprintService _sprintService;

        public SprintController(ISprintService sprintService)
        {
            _sprintService = sprintService;
        }

        [QuyenHan("SPRINT_VIEW")]
        [HttpGet("du-an/{projectId}")]
        public async Task<IActionResult> LayTheoDuAn(int projectId)
        {
            try
            {
                var result = await _sprintService.GetByProjectIdAsync(projectId);
                return SuccessResponse(result, "Lay danh sach sprint theo du an thanh cong.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [QuyenHan("SPRINT_VIEW")]
        [HttpGet("{id}")]
        public async Task<IActionResult> ChiTiet(int id)
        {
            try
            {
                var result = await _sprintService.GetByIdAsync(id);
                if (result == null) return ErrorResponse(404, "Khong tim thay sprint.");
                return SuccessResponse(result, "Lay thong tin sprint thanh cong.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [QuyenHan("SPRINT_CREATE")]
        [HttpPost("tao-sprint")]
        public async Task<IActionResult> TaoSprint([FromBody] TaoSprintDto dto)
        {
            try
            {
                var result = await _sprintService.CreateAsync(dto, CurrentUserId);
                return SuccessResponse(result, "Tao sprint thanh cong.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [QuyenHan("SPRINT_UPDATE")]
        [HttpPut("{id}")]
        public async Task<IActionResult> CapNhat(int id, [FromBody] CapNhatSprintDto dto)
        {
            try
            {
                var result = await _sprintService.UpdateAsync(id, dto);
                if (result) return SuccessResponse(null!, "Cap nhat sprint thanh cong.");
                return ErrorResponse(400, "Khong the cap nhat sprint.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [QuyenHan("SPRINT_UPDATE")]
        [HttpPost("{id}/kich-hoat")]
        public async Task<IActionResult> KichHoatSprint(int id)
        {
            try
            {
                // Gọi service kích hoạt Sprint, truyền userId của PM đang đăng nhập
                var result = await _sprintService.KichHoatSprintAsync(id, CurrentUserId);
                if (result == null) return ErrorResponse(404, "Không tìm thấy Sprint.");
                return SuccessResponse(result, "Kích hoạt Sprint thành công. Sprint đang chạy.");
            }
            catch (InvalidOperationException ex)
            {
                // Lỗi nghiệp vụ (Sprint đã chạy / đã kết thúc)
                return ErrorResponse(400, ex.Message);
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [QuyenHan("SPRINT_DELETE")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Xoa(int id)
        {
            try
            {
                var result = await _sprintService.DeleteAsync(id);
                if (result) return SuccessResponse(null!, "Xoa sprint thanh cong.");
                return ErrorResponse(400, "Khong the xoa sprint.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }
    }
}
