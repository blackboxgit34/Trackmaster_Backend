using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using Trackmaster_Model;
using Trackmaster_Service.Interface;
namespace Trackmaster_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IHttpClientFactory _httpClientFactory;
        public AccountController(IAccountService accountService, IHttpClientFactory httpClientFactory)
        {
            _accountService = accountService;
            _httpClientFactory = httpClientFactory;
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
        [HttpGet("VerifyUser")]
        public async Task<IActionResult> VerifyUser(string username, string website)
        {
            try
            {
                var url = $"https://api1.trackmaster.in/api/CommonApi/VerifyUser?username={username}&website={website}";

                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "External API failed");
                }

                var content = await response.Content.ReadAsStringAsync();

                var data = JsonSerializer.Deserialize<UserOtp>(content);

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("VerifyUserOtp")]
        public async Task<IActionResult> VerifyUserOtp(int custid, string website, string OTP)
        {
            UserOtp response = _accountService.VerifyUserOtp(custid, website, OTP);
            if (response.message != "Failed")
            {
                return Ok(response);
            }
            else
            {
                return Ok(response);
            }
        }
        [HttpGet("OTPChangePassword")]
        public async Task<IActionResult> OTPChangePassword(string custId, string NewPassword)
        {
            var result = _accountService.UpdateOTPAdminPassword(custId, NewPassword);
            return Ok(result);
        }
    }
}
