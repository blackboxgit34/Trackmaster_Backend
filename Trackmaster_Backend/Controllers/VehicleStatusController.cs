using Microsoft.AspNetCore.Mvc;
using Trackmaster_Model;
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
        public async Task<IActionResult> GetvehicleStatusList(string pagename,  [FromQuery] DataTableRequestModel model)
        {
            try
            {
                int sEcho = model.sEcho; 
                int start = model.iDisplayStart;
                int length = model.iDisplayLength;
                string search = model.sSearch;
                string sortColumn = model.sortColumn;
                string sortDirection = model.sortDirection;


                var vehiclestatuslist = await _vehiclestatusService.GetvehicleStatusList(pagename,model);
                return Ok(new
                {
                    success = true,
                    message = "Vehicle data retrieved successfully",
                    data = vehiclestatuslist,

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
