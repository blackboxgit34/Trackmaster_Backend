using Microsoft.AspNetCore.Mvc;
using Trackmaster_Service.Interface;

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
    }
}
