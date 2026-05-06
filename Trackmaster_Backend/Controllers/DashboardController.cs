using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Net;
using Trackmaster_Model;
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
        public async Task<IActionResult> GetDashboardData(int userid, string type = null)
        {
            try
            {
                var dashboardData = await _dashboardService.GetDashboardData(userid, type);
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



        [HttpGet("GetAllVehicleListByCustId")]
        public IActionResult GetAllVehicleListByCustId(int custId)
        {
            try
            {
                // Call the service method to get dashboard data
                var dashboardData = _dashboardService.GetAllVehicleListByCustId(custId);
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


        [HttpGet("GetOverSpeedGraphReport")]
        public IActionResult GetOverSpeedGraphReport(int custid,string bbid=null)
        {
            try
            {
                OverSpeedReport overspeedR = new OverSpeedReport();
                overspeedR = _dashboardService.GetOverSpeedGraphReport(custid,bbid);
                if (overspeedR != null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Overspeed graph report retrieved successfully",
                        data = overspeedR
                    });
                }
                else
                {
                    return NoContent();
                }
            }
            catch (Exception)
            {

                throw;
            }
          
        }
    }
}