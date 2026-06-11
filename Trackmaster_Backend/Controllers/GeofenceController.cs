using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Trackmaster_Model;
using Trackmaster_Service.Interface;

namespace Trackmaster_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeofenceController : ControllerBase
    {
        private readonly IGeofenceService _geofenceService;
        private readonly IDashboardService _dashboardService;
        public GeofenceController(IGeofenceService geofenceService, IDashboardService dashboardService)
        {
            _geofenceService = geofenceService;
            _dashboardService = dashboardService;
        }
        [HttpPost("SaveGeofence")]
        public async Task<IActionResult> SaveGeofence(GeofenceModel model)
        {
            try
            {
                var result = await _geofenceService.SaveGeofence(model);
                return StatusCode(200, new
                {
                    message = result,
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
        [HttpGet("GetGeofenceList")]
        public async Task<IActionResult> GetGeofenceList([FromQuery] DataTableRequestModel model)
        {
            try
            {
                var result = await _geofenceService.GetGeofenceList(model);
                var vehicleListResult = await _dashboardService.GetAllVehicleListByCustId(model.CustId);
                return Ok(new
                {
                    data = result.geofenceList,
                    count = result.TotalCount,
                    vehicleList = vehicleListResult
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
