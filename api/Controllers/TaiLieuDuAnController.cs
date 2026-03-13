using Apllication.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaiLieuDuAnController : BaseController
    {
        private readonly ITaiLieuDuAnService _taiLieuService;

        public TaiLieuDuAnController(ITaiLieuDuAnService taiLieuService)
        {
            _taiLieuService = taiLieuService;
        }

        [HttpPost("upload/{duAnId}")]
        public async Task<IActionResult> Upload(int duAnId, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return ErrorResponse(400, "Vui long chon file de tai len.");

                // Lay UserId tu Token (Tam thoi gia dinh la 1 neu chua co Auth)
                int userId = 1;
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null) userId = int.Parse(userIdClaim.Value);

                var result = await _taiLieuService.UploadAsync(duAnId, file, userId);
                return SuccessResponse(result, "Tai tai lieu len thanh cong.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [HttpGet("du-an/{projectId}")]
        public async Task<IActionResult> LayTheoDuAn(int projectId)
        {
            try
            {
                var result = await _taiLieuService.GetByProjectIdAsync(projectId);
                return SuccessResponse(result, "Lay danh sach tai lieu thanh cong.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }

        [HttpPost("{id}/xu-ly-ai")]
        public async Task<IActionResult> XuLyAi(int id)
        {
            try
            {
                var result = await _taiLieuService.ProcessWithAiAsync(id);
                if (result) return SuccessResponse(null!, "Kich hoat AI xu ly tai lieu thanh cong.");
                return ErrorResponse(400, "Khong the xu ly tai lieu nay hoặc tài liệu đã được xử lý.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }
    }
}
