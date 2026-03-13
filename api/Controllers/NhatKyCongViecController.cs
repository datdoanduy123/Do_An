using Apllication.IService;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NhatKyCongViecController : BaseController
    {
        private readonly INhatKyCongViecService _taskLogService;

        public NhatKyCongViecController(INhatKyCongViecService taskLogService)
        {
            _taskLogService = taskLogService;
        }

        [HttpGet("cong-viec/{taskId}")]
        public async Task<IActionResult> GetByTask(int taskId)
        {
            try
            {
                var result = await _taskLogService.GetLogsByTaskIdAsync(taskId);
                return SuccessResponse(result, "Lay lich su cong viec thanh cong.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, ex.Message);
            }
        }
    }
}
