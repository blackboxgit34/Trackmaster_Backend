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

        [HttpPost("AddPOI")]
        public async Task<IActionResult> AddPOI([FromBody] AddPoiRequest request)
        {
            request.location = request.location.Replace(";", "");
            request.location = request.location.Replace("&", "&amp;");

            bool locationNotExists = await _geofenceService.LocationExist(Convert.ToSingle(request.lat), Convert.ToSingle(request.longi), Convert.ToInt32(request.custid));

            if (locationNotExists)
            {
                var result = await _geofenceService.SavePOI(Convert.ToDouble(request.lat), Convert.ToDouble(request.longi), Convert.ToInt32(request.custid), request. location, request.radius);
                if (result)
                {
                    return StatusCode(200, new
                    {
                        message = "POI added successfully",
                    });
                }
                else
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Failed to add POI",
                    });
                }
            }
            else
            {
                return StatusCode(400, new
                {
                    success = false,
                    message = "Location already exists",
                });

            }
        }

        [HttpGet("GetPOI")]

        public async Task<IActionResult> GetPOI(string CustId)
        {
            try
            {
                var getpoilist = await _geofenceService.GetPOi(CustId);
                if (getpoilist !=null)
                {
                    return StatusCode(200, new
                    {
                        success = true,
                        data = getpoilist,
                        message = "POI fetched successfully"
                    });
                }
                else
                {
                    return StatusCode(404, new
                    {
                        success = false,
                        data = new List<PoiList>(),
                        message = "No POI found"
                    });
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpPost("ManagePoi")]
        public async Task<IActionResult> ManagePoi([FromQuery] DataTableRequestModel request, string? id = null)
        {
            try
            {
                var result = await _geofenceService.ManagePoi(request, id);

                if (result != null && result.Data.Any())
                {
                    return StatusCode(200, new
                    {
                        success = true,
                        data = result,
                        message = "POI fetched successfully"
                    });
                }

                return StatusCode(404, new
                {
                    success = false,
                    data = result,
                    message = "No POI found"
                });
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
