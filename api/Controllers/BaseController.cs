using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BaseController : ControllerBase
    {
        // Helper method de tra ve ket qua thanh cong
        protected IActionResult SuccessResponse(object data, string message = "Thanh cong")
        {
            return Ok(new
            {
                StatusCode = 200,
                Message = message,
                Data = data
            });
        }

        // Helper method de tra ve loi voi format chung
        protected IActionResult ErrorResponse(int statusCode, string message, object? details = null)
        {
            return StatusCode(statusCode, new
            {
                StatusCode = statusCode,
                Message = message,
                Details = details
            });
        }

        // Wrapper de xu ly try-catch cho cac Action (neu can dung thu cong)
        protected async Task<IActionResult> ExecuteActionAsync<T>(Func<Task<T>> action)
        {
            try
            {
                var result = await action();
                return SuccessResponse(result!);
            }
            catch (UnauthorizedAccessException ex)
            {
                return ErrorResponse(401, "Ban khong co quyen truy cap.", ex.Message);
            }
            catch (Exception ex)
            {
                // Mac dinh la 500 neu khong xac dinh duoc loai loi
                return ErrorResponse(500, "Da co loi xay ra trong he thong.", ex.Message);
            }
        }
    }
}
