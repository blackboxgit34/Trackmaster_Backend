using Microsoft.AspNetCore.Mvc;
using Trackmaster_Service.Interface;

namespace Trackmaster_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("dashboarddata")]
        public async Task<IActionResult> GetDashboardData(int userid)
        {
            try 
            {
                var dashboardData = await _dashboardService.GetDashboardData(userid);
                return Ok(dashboardData);
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