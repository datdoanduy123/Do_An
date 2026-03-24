using Apllication.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ThongBaoController : ControllerBase
    {
        private readonly IThongBaoService _service;

        public ThongBaoController(IThongBaoService service)
        {
            _service = service;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var result = await _service.LayThongBaoTheoUserAsync(userId);
            return Ok(result);
        }

        [HttpGet("unread-count/{userId}")]
        public async Task<IActionResult> GetUnreadCount(int userId)
        {
            var count = await _service.LaySoLuongChuaDocAsync(userId);
            return Ok(count);
        }

        [HttpPost("mark-read/{id}")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var success = await _service.DanhDauDaDocAsync(id);
            return Ok(success);
        }

        [HttpPost("mark-all-read/{userId}")]
        public async Task<IActionResult> MarkAllRead(int userId)
        {
            var success = await _service.DanhDauTatCaDaDocAsync(userId);
            return Ok(success);
        }

        [HttpDelete("all/{userId}")]
        public async Task<IActionResult> DeleteAll(int userId)
        {
            var success = await _service.XoaTatCaThongBaoAsync(userId);
            return Ok(success);
        }
    }
}
