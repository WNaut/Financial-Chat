using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialChat.WebApp.Helpers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class CustomController : Controller
    {
        [NonAction]
        public IActionResult CustomOk(object data, string message = "")
        {
            return Ok(new BaseResponse(data, message));
        }
        
        [NonAction]
        public IActionResult CustomBadRequest(string message)
        {
            return BadRequest(new BaseResponse(message));
        }
    }
}