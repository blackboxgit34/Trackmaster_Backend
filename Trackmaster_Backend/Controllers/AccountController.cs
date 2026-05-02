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
        public async Task<IActionResult> AuthorizeUser(string userId, string password, string type)
        {
            try
            {
                var user = _accountService.AuthorizeUser(userId, password, type);

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
        [HttpGet("GetUserBySearching")]
        public async Task<IActionResult> GetUserBySearching(string search)
        {
            try
            {
                var user = _accountService.GetUserBySearching(search);
                return Ok(user);
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
