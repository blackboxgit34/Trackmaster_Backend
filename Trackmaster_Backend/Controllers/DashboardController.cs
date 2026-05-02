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
        public IActionResult GetDashboardData(int userid)
        {
            try
            {
                // Call the service method to get dashboard data
                var dashboardData = _dashboardService.GetDashboardData(userid);
                return Ok(new
                {
                    success = true,
                    message = "Dashboard data retrieved successfully",
                    data = dashboardData
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