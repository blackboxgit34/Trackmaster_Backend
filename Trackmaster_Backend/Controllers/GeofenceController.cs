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
        public GeofenceController(IGeofenceService geofenceService)
        {
            _geofenceService = geofenceService;
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
    }
}
