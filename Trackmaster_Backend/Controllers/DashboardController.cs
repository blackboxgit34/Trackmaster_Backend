using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System;
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
        public async Task<IActionResult> GetDashboardData(int userid, string type = null, string bbid = null, DateTime start, DateTime end)
        {
            try
            {
                var dashboardData = await _dashboardService.GetDashboardData(userid, type, bbid, start, end);
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
        public async Task<IActionResult> GetAllVehicleListByCustId(int userid)
        {
            try
            {
                var dashboardData = await _dashboardService.GetAllVehicleListByCustId(userid);
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