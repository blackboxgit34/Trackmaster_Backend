using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using System;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;
using Trackmaster_Model;
using Trackmaster_Repository.Repository;
using Trackmaster_Service;
using Trackmaster_Service.Interface;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Trackmaster_Model.Reports;
using static Trackmaster_Service.ImportExportExcelService;

namespace Trackmaster_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IReportsService _reportsService;
        private readonly IWebHostEnvironment _environment;
        private readonly ImportExportExcelService _importExportExcelService;
        private readonly ImportExportPdfService _importExportPdfService;
        public ReportsController(IReportsService reportsService, IWebHostEnvironment environment, ImportExportExcelService importExportExcelService, ImportExportPdfService importExportPdfService)
        {
            _reportsService = reportsService;
            _environment = environment;
            _importExportExcelService = importExportExcelService;
            _importExportPdfService = importExportPdfService;
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
            var modelObj = await _reportsService.GetConductorInfo(requestModel);

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
        [HttpGet("GetDesignationTypeCrew")]
        public async Task<IActionResult> GetDesignationTypeCrew()
        {
            List<DropDownItems> empTypeList = await _reportsService.GetDesignationTypeCrew();

            var aaData = empTypeList;
            return Ok(new { aaData = empTypeList });
        }

        [HttpGet("GetStatesList")]
        public async Task<IActionResult> GetStatesList()
        {
            List<DropDownItems> stateList = await _reportsService.GetStatesList();
            var aaData = stateList;
            return Ok(new { aaData = stateList });
        }

        [HttpGet("GetCityList")]
        public async Task<IActionResult> GetCityList(int stateid)
        {
            List<DropDownItems> cityList = await _reportsService.GetCityList(stateid);
            var cityData = cityList;
            return Ok(new { cityData = cityList });
        }


        [HttpPost("AddUpdateEmployee")]
        public async Task<IActionResult> AddUpdateEmployee([FromForm] Employee objEmp)
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
                var result = await _reportsService.AddUpdateEmployee(objEmp, finalImagePath);

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
        [HttpGet("GetMessageType")]
        public async Task<IActionResult> GetMessageType()
        {
            try
            {
                var messageTypeData = await _reportsService.GetMessageType();
                return Ok(new
                {
                    success = true,
                    message = "message type data retrieved successfully",
                    data = messageTypeData
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


        [HttpGet("GetMessageReports")]
        public async Task<IActionResult> GetMessageReports([FromQuery] DataTableRequestModel requestModel, string typeid, string messagetype, string vehicleNo, string downloadType = null)
        {
            try
            {
                SMSReportEx sms = await _reportsService.GetSentMessagesReport(requestModel, Convert.ToInt32(typeid), messagetype, vehicleNo);

                // ================= EXCEL EXPORT =================

                if (downloadType == "Excel")
                {
                    var reportName = $"SMSNotificationReport_{DateTime.Now:yyyyMMdd}.xlsx";
                    var exportData = sms?.objSMSReport?.ToList();
                    var stream = await _importExportExcelService.ExportToExcelFlatList(exportData, reportName, null, null);
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", reportName);
                }
                // ================= PDF EXPORT =================
                else if (downloadType == "PDF")
                {
                    var reportName = $"SMSNotificationReport_{DateTime.Now:yyyyMMdd}.pdf";
                    var exportData = sms?.objSMSReport?.ToList();
                    var stream = await _importExportPdfService.ExportToPdfFlatList(exportData, reportName, null, null);
                    return File(stream, "application/pdf", reportName);
                }
                // ================= NORMAL RESPONSE =================
                if (sms != null)
                {
                    return Ok(new
                    {
                        sEcho = requestModel.sEcho,
                        iTotalRecords = sms.pagecount,
                        iTotalDisplayRecords = sms.pagecount,
                        aaData = sms.objSMSReport
                    });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message =
                            "An error occurred while fetching message reports.",
                        error = ex.Message
                    }
                );
            }
        }

        [HttpGet("VehicleStatus")]
        public async Task<IActionResult> VehicleStatus(int custId, int lower, int upper, string? search, DateTime start, DateTime end)
        {
            var result = await _reportsService.VehicleStatus(custId, lower, upper, search, start, end);

            return Ok(new
            {
                data = result.VehicleData,
                count = result.ItemCount
            });
        }

        [HttpGet("BatteryDisconnection")]
        public async Task<IActionResult> BatteryDisconnection(int custId, int lower, int upper, string? search, DateTime start, DateTime end)
        {
            var result = await _reportsService.BatteryDisconnection(custId, lower, upper, search, start, end);

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

            if (model.DownloadType == "Excel")
            {
                var reportName = $"DistanceReport_{model.CustId}.xlsx";
                var stream = await _importExportExcelService.ExportToExcelFlatList(result.data, reportName, null, null);
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", reportName);
            }
            if (model.DownloadType == "Pdf")
            {
                var reportName = $"DistanceReport_{model.CustId}.pdf";
                var stream = await _importExportPdfService.ExportToPdfFlatList(result.data, reportName, null, null);
                return File(stream, "application/pdf", reportName);
            }
            return Ok(new
            {
                data = result.data,
                count = result.TotalCount
            });
        }

        [HttpGet("GetAllStoppageReport")]
        public async Task<IActionResult> GetAllStoppageReport([FromQuery] DataTableRequestModel dtmodel)
         {
           
           var stoppage = await _reportsService.GetCombinedStoppageReport(dtmodel);

            return Ok(new
            {
                data = stoppage.data,
                count = stoppage.TotalCount
            });

        }

        [HttpGet("GetIdlingStatusReport")]
        public async Task<IActionResult> GetIdlingStatusReport([FromQuery] DataTableRequestModel dtmodel)
        {

            var stoppage = await _reportsService.GetIdlingStatusReport(dtmodel);

            return Ok(new
            {
                data = stoppage.data,
                count = stoppage.TotalCount
            });

        }

        [HttpPost("GetMonthlyDistanceReportData")]
        public async Task<IActionResult> GetMonthlyDistanceReportData([FromBody] DataTableRequestModel model)
        {
            var result = await _reportsService.GetMonthlyDistanceReportData(model);

            if (model.DownloadType == "Excel")
            {
                var reportName = $"MonthlyDistanceReport_{model.CustId}.xlsx";
                var stream = await _importExportExcelService.ExportToExcelFlatList(result.data, reportName, null, null);
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", reportName);
            }
            if (model.DownloadType == "Pdf")
            {
                var reportName = $"MonthlyDistanceReport_{model.CustId}.pdf";
                var stream = await _importExportPdfService.ExportToPdfFlatList(result.data, reportName, null, null);
                return File(stream, "application/pdf", reportName);
            }

            return Ok(new
            {
                data = result.data,
                count = result.TotalCount
            });
        }
        #region Neha Vaid
        [HttpGet("getSpeedReport")]
        public async Task<IActionResult> getSpeedReport(string mode, [FromQuery] DataTableRequestModel requestModel)
        {

            try
            {
                var speedData = await _reportsService.getSpeedReport(mode, requestModel);
                if (speedData != null)
                {
                    return Ok(new
                    {
                        sEcho = requestModel.sEcho,
                        iTotalRecords = speedData.PageCount,
                        iTotalDisplayRecords = speedData.PageCount,
                        aaData = speedData
                    });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while fetching speed data.",
                    error = ex.Message
                });
            }
        }
        #endregion
        [HttpPost("GetEntryExitReport")]
        public async Task<IActionResult> GetEntryExitReport([FromQuery] DataTableRequestModel model, [FromQuery] string bbid = "")
        {
            var stoppage = await _reportsService.GetListofEntryExit(model, bbid);

            if(model.DownloadType == "Excel")
            {
                var reportName = $"EntryExitReport_{model.CustId}.xlsx";
                var stream = await _importExportExcelService.ExportToExcelFlatList(stoppage.vehicleList, reportName, null, null);
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", reportName);
            }
            if(model.DownloadType == "Pdf")
            {
                var reportName = $"EntryExitReport_{model.CustId}.pdf";
                var stream = await _importExportPdfService.ExportToPdfFlatList(stoppage.vehicleList, reportName, null, null);
                return File(stream, "application/pdf", reportName);
            }

            return Ok(new
            {
                data = stoppage.vehicleList,
                count = stoppage.PageCount
            });
        }
    }
}
