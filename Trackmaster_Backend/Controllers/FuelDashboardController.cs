using Microsoft.AspNetCore.Mvc;
using Trackmaster_Service.Interface;

namespace Trackmaster_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FuelDashboardController : Controller
    {
        private readonly IFuelDashboardService _fueldashboardService;
        public FuelDashboardController(IFuelDashboardService fueldashboardService)
        {
            _fueldashboardService = fueldashboardService;
        }


        [HttpGet("FuelDashboardData")]
        public async Task<IActionResult> GetFuelDashboardData(int custid)
        {
            try
            {
                var dashboardData = await _fueldashboardService.GetCurrentFuelData(custid);
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
