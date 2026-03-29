using Apllication.DTOs.QuyTacGiaoViecAI;
using Apllication.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class QuyTacGiaoViecAIController : BaseController
    {
        private readonly IQuyTacGiaoViecAIService _service;

        public QuyTacGiaoViecAIController(IQuyTacGiaoViecAIService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuyTacGiaoViecAIDto>>> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<QuyTacGiaoViecAIDto>> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "QUAN_LY")]
        public async Task<IActionResult> Update(int id, CapNhatQuyTacGiaoViecAIDto dto)
        {
            var success = await _service.UpdateAsync(id, dto);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
