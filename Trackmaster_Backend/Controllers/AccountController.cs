using Microsoft.AspNetCore.Mvc;
using Trackmaster_Service.Interface;
namespace Trackmaster_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }
        [HttpGet("login")]
        public IActionResult AuthorizeUser(string userId, string password)
        {
            try
            {
                var user = _accountService.AuthorizeUser(userId, password);

                if (!user.IsSuccess)
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = user.Message
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = user.Message,
                    data = user
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal Server Error",
                    error = ex.Message
                });
            }
        }
    }
}
