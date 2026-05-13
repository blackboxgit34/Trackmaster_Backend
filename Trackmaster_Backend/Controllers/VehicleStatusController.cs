using Microsoft.AspNetCore.Mvc;
using Trackmaster_Service.Interface;
using Trackmaster_Service.Service;

namespace Trackmaster_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleStatusController : ControllerBase
    {
        private readonly IVehicleStatusService _vehiclestatusService;
        public VehicleStatusController(IVehicleStatusService vehiclestatusService)
        {
            _vehiclestatusService = vehiclestatusService;
        }

        [HttpGet("GetvehicleStatusList")]
        public async Task<IActionResult> GetvehicleStatusList(int userid)
        {
            try
            {
                var vehiclestatuslist = await _vehiclestatusService.GetvehicleStatusList(userid);
                return Ok(new
                {
                    success = true,
                    message = "Vehicle data retrieved successfully",
                    data = vehiclestatuslist
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
        [HttpGet("GetPlaybackData")]
        public async Task<IActionResult> GetPlaybackData(string bbid, DateTime date)
        {
            try
            {
                var dashboardData = await _vehiclestatusService.GetPlaybackData(bbid, date);
                return Ok(new
                {
                    success = true,
                    message = "Vehicle data retrieved successfully",
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
