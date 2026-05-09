using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Xml.Linq;
using Trackmaster_Service.Interface;
using static Trackmaster_Model.Reports;

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
        /// <summary>
        /// Get crew report data
        /// </summary>
        /// <param name="CustId"></param>
        /// <param name="sEcho"></param>
        /// <param name="iDisplayStart"></param>
        /// <param name="iDisplayLength"></param>
        /// <param name="sSearch"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <returns></returns>
        [HttpGet("GetConductorInfo")]
        public async Task<IActionResult> GetConductorInfo(int CustId,int sEcho,int iDisplayStart,int iDisplayLength,string sSearch,string sortColumn,string sortDirection)
        {
            var modelObj = _reportsService.GetConductorInfo(CustId, sEcho, iDisplayStart, iDisplayLength, sSearch, sortColumn, sortDirection);

            if (modelObj == null)
                return NoContent();

            return Ok(new
            {
                sEcho = sEcho,
                iTotalRecords = modelObj.PageCount,
                iTotalDisplayRecords = modelObj.PageCount,
                aaData = modelObj.modelObjList
            });
        }
        /// <summary>
        /// Get Designation list
        /// </summary>
        /// <returns></returns>
        [HttpGet ("GetDesignationTypeCrew")]
        public async Task<IActionResult> GetDesignationTypeCrew()
        {
            List<DropDownItems> empTypeList = _reportsService.GetDesignationTypeCrew();

            var aaData = empTypeList;
            return Ok(new { aaData = empTypeList });
        }

        [HttpGet("GetStatesList")]
        public async Task<IActionResult> GetStatesList()
        {
            List<DropDownItems> stateList = _reportsService.GetStatesList();
            var aaData = stateList;
            return Ok(new { aaData = stateList });
        }

        [HttpGet("GetCityList")]
        public async Task<IActionResult> GetCityList(int stateid)
        {
            List<DropDownItems> cityList = _reportsService.GetCityList(stateid);
            var cityData = cityList;
            return Ok(new { cityData = cityList }); 
        }

        [HttpPost("AddUpdateEmployee")]
        public async Task<IActionResult>  AddUpdateEmployee([FromBody] Employee employee)
        {
            string isInsertUpdate = "";

            isInsertUpdate = _reportsService.AddUpdateEmployee(employee);

            return Ok(isInsertUpdate);
        }


    }
}
