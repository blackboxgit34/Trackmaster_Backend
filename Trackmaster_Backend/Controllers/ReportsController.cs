using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Xml.Linq;
using Trackmaster_Model;
using Trackmaster_Service.Interface;
using static Trackmaster_Model.Reports;

namespace Trackmaster_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IReportsService _reportsService;
        private readonly IWebHostEnvironment _environment;
        public ReportsController(IReportsService reportsService, IWebHostEnvironment environment)
        {
            _reportsService = reportsService;
            _environment = environment;
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
        public async Task<IActionResult> GetConductorInfo([FromQuery] DataTableRequestModel requestModel)
        {
            var modelObj = _reportsService.GetConductorInfo(requestModel);

            if (modelObj == null)
                return NoContent();

            return Ok(new
            {
                sEcho = requestModel.sEcho,
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
        public IActionResult AddUpdateEmployee([FromForm] Employee objEmp)
        {
            try
            {
                var folderName = "FileUpload";
                var uploadPath = Path.Combine(_environment.WebRootPath, folderName);

                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                List<DocInfo> fileList = new List<DocInfo>();
                List<string> imagePaths = new List<string>();

                var files = objEmp.ImageFiles;

                if (files != null && files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        if (file.Length > 0)
                        {
                            string fileName = Guid.NewGuid().ToString("N").Substring(0, 6)
                                              + "_" + file.FileName;

                            string physicalPath = Path.Combine(uploadPath, fileName);

                            using (var stream = new FileStream(physicalPath, FileMode.Create))
                            {
                                file.CopyTo(stream);
                            }

                            fileList.Add(new DocInfo
                            {
                                Name = file.FileName,
                                fullPath = "/FileUpload/" + fileName
                            });

                            imagePaths.Add("/FileUpload/" + fileName);
                        }
                    }
                }

                string finalImagePath = string.Join(",", imagePaths);
                var result = _reportsService.AddUpdateEmployee(objEmp, finalImagePath);

                return Ok(new
                {
                    message = result,
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("vehicle-status")]
        public IActionResult VehicleStatus(int custId, int lower, int upper, string? search, DateTime start, DateTime end)
        {
            var result = _reportsService.VehicleStatus(custId, lower, upper, search, start, end);

            return Ok(new
            {
                data = result.VehicleData,
                count = result.ItemCount
            });
        }

        [HttpPost("GetDistanceReportData")]
        public async Task<IActionResult> GetDistanceReportData([FromBody] DataTableRequestModel model)
        {
            var result = await _reportsService.GetDistanceReportData(model);

            return Ok(new
            {
                data = result,
                count = result.Count()
            });
        }

    }
}
