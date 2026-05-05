using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Trackmaster_Service.Interface;

namespace Trackmaster_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IReportsService _reportsService;
        public ReportsController(IReportsService reportsService)
        {
            _reportsService = reportsService;
        }

        [HttpGet("GetConductorInfo")]
        public async Task<IActionResult> GetConductorInfo(int CustId, int sEcho, int iDisplayStart, int iDisplayLength, string sSearch)
        {
            var lowerBound = iDisplayStart;
            var upperBound = iDisplayLength;

            var modelObj = _reportsService.GetConductorInfo(CustId, lowerBound, upperBound, sSearch);
            if (modelObj == null)
            {
                return NoContent();
            }
            return Ok(new
            {
                sEcho = sEcho,
                iTotalRecords = modelObj.PageCount,
                iTotalDisplayRecords = modelObj.PageCount,
                aaData = modelObj.modelObjList
            });
        }

    }
}
