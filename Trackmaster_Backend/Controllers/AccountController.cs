using Microsoft.AspNetCore.Http;
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
    }
}
