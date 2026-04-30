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
        [AcceptVerbs("GET")]
        public async Task<IActionResult> AuthorizeUser(string userId, string password)
        {
            try
            {
                var user = _accountService.AuthorizeUser(userId, password);

                if (user == null)
                {
                    return Unauthorized(new
                    {
                        message = "Invalid Username or Password!"
                    });
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
