using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using System;
using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Trackmaster_Model;
using Trackmaster_Repository.Repository;
using Trackmaster_Service;
using Trackmaster_Service.Interface;
using Trackmaster_Service.Service;
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
        private readonly IConfiguration _configuration;
        private readonly IMongoService _mongoService;


        public ReportsController(IReportsService reportsService, IWebHostEnvironment environment, ImportExportExcelService importExportExcelService, ImportExportPdfService importExportPdfService, IConfiguration configuration, IMongoService mongoService)
        {
            _reportsService = reportsService;
            _environment = environment;
            _importExportExcelService = importExportExcelService;
            _importExportPdfService = importExportPdfService;
            _configuration = configuration;
            _mongoService = mongoService;
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
                    //var reportName = $"SMSNotificationReport_{DateTime.Now:yyyyMMdd}.xlsx";
                    var reportName = $"SMSNotificationReport_{requestModel.beginDate:yyyyMMdd}_to_{requestModel.endDate:yyyyMMdd}_{DateTime.Now:HHmmss}.xlsx";
                    var exportData = sms?.objSMSReport?.ToList();
                    var stream = await _importExportExcelService.ExportToExcelFlatList(exportData, reportName, null, null);
                    Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");
                    Response.Headers["Content-Disposition"] = $"attachment; filename={reportName}";
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", reportName);
                }
                // ================= PDF EXPORT =================
                else if (downloadType == "Pdf")
                {
                    //var reportName = $"SMSNotificationReport_{DateTime.Now:yyyyMMdd}.pdf";
                    var reportName = $"SMSNotificationReport_{requestModel.beginDate:ddMMMyyyy}_To_{requestModel.endDate:ddMMMyyyy}.pdf";
                    var exportData = sms?.objSMSReport?.ToList();
                    var stream = await _importExportPdfService.ExportToPdfFlatList(exportData, reportName, null, null);
                    //  IMPORTANT: expose header to frontend
                    Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");
                    //  IMPORTANT: force filename into response header
                    Response.Headers["Content-Disposition"] = $"attachment; filename={reportName}";
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
        // neha k
        [HttpGet("GetConsolidatedIgnitionStatus")]
        public async Task<IActionResult> GetConsolidatedIgnitionStatus([FromQuery] DataTableRequestModel requestModel, string bbid, string reportName, string downloadType = null)

        {
            try
            {
                int lowerBound = requestModel.iDisplayStart;
                int upperBound = requestModel.iDisplayStart + requestModel.iDisplayLength;

                if (upperBound == 0)
                    upperBound = 20;

                ConsolidatedIgnitionModel consIgnition = await _reportsService.GetConsolidatedIgnitionStatus(requestModel, bbid, reportName);

                if (consIgnition == null ||
                    consIgnition.ConsolidatedIgnitionList == null)
                {
                    return NoContent();
                }

                // ================= EXCEL EXPORT =================
                if (downloadType == "Excel")
                {
                    //var fileName =$"ConsolidatedIgnitionStatus_{DateTime.Now:yyyyMMdd}.xlsx";
                    var fileName = $"Ignition On/Off Analysis_{requestModel.beginDate:yyyyMMdd}_to_{requestModel.endDate:yyyyMMdd}_{DateTime.Now:HHmmss}.xlsx";
                    var exportData = consIgnition.ConsolidatedIgnitionList.ToList();

                    var stream = await _importExportExcelService.ExportToExcelFlatList(exportData, fileName, null, null);
                    Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");
                    Response.Headers["Content-Disposition"] = $"attachment; filename={fileName}";
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }

                // ================= PDF EXPORT =================
                if (downloadType == "Pdf")
                {
                    var fileName = $"Ignition On/Off Analysis_{requestModel.beginDate:yyyyMMdd}_to_{requestModel.endDate:yyyyMMdd}_{DateTime.Now:HHmmss}.pdf";
                    var exportData = consIgnition.ConsolidatedIgnitionList.ToList();
                    var stream = await _importExportPdfService.ExportToPdfFlatList(exportData, "Ignition On/Off Analysis Report", fileName, bbid);
                    // expose header to frontend
                    Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");
                    // force filename into response header
                    Response.Headers["Content-Disposition"] = $"attachment; filename={fileName}";

                    return File(stream, "application/pdf", fileName);
                }

                // ================= NORMAL RESPONSE =================
                return Ok(new
                {
                    sEcho = requestModel.sEcho,
                    iTotalRecords = consIgnition.PageCount,
                    iTotalDisplayRecords = consIgnition.PageCount,
                    aaData = consIgnition.ConsolidatedIgnitionList
                });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = "An error occurred while fetching consolidated ignition status.",
                        error = ex.Message
                    });
            }
        }



        [HttpGet("VehicleStatus")]
        public async Task<IActionResult> VehicleStatus([FromQuery] DataTableRequestModel model)
        {
            try
            {
                var result = await _reportsService.VehicleStatus(model);

                if (result == null)
                    return NoContent();

                if (model.DownloadType == "Excel")
                {
                    var reportName = $"VehicleStatus_{model.CustId}.xlsx";

                    var stream = await _importExportExcelService.ExportToExcelFlatList(result.VehicleData, reportName, null, null);

                    return File(
                        stream,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        reportName);
                }

                if (model.DownloadType == "Pdf")
                {
                    var reportName = $"VehicleStatus_{model.CustId}.pdf";

                    var stream = await _importExportPdfService.ExportToPdfFlatList(result.VehicleData, reportName, null, null);

                    return File(
                        stream,
                        "application/pdf",
                        reportName);
                }

                return Ok(new
                {
                    data = result.VehicleData,
                    count = result.ItemCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while fetching vehicle status data.",
                    error = ex.Message
                });
            }
        }

        [HttpGet("BatteryDisconnection")]
        public async Task<IActionResult> BatteryDisconnection([FromQuery] DataTableRequestModel model)
        {
            try
            {
                var result = await _reportsService.BatteryDisconnection(model);

                if (result == null)
                    return NoContent();

                if (model.DownloadType == "Excel")
                {
                    var reportName = $"BatteryDisconnection_{model.CustId}.xlsx";

                    var stream = await _importExportExcelService.ExportToExcelFlatList(result.VehicleData, reportName, null, null);

                    return File(
                        stream,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        reportName);
                }

                if (model.DownloadType == "Pdf")
                {
                    var reportName = $"BatteryDisconnection_{model.CustId}.pdf";

                    var stream = await _importExportPdfService.ExportToPdfFlatList(result.VehicleData, reportName, null, null);

                    return File(
                        stream,
                        "application/pdf",
                        reportName);
                }

                return Ok(new
                {
                    data = result.VehicleData,
                    count = result.ItemCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while fetching battery disconnection data.",
                    error = ex.Message
                });
            }
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
            try
            {
                var stoppage = await _reportsService.GetCombinedStoppageReport(dtmodel);
                if (dtmodel.DownloadType == "Excel")
                {
                    var reportName = $"StoppageReport_{dtmodel.CustId}.xlsx";
                    var stream = await _importExportExcelService.ExportToExcelFlatList(stoppage.data, reportName, null, null);
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", reportName);
                }
                if (dtmodel.DownloadType == "Pdf")
                {
                    var reportName = $"StoppageReport_{dtmodel.CustId}.pdf";
                    var stream = await _importExportPdfService.ExportToPdfFlatList(stoppage.data, reportName, null, null);
                    return File(stream, "application/pdf", reportName);
                }

                return Ok(new
                {
                    data = stoppage.data,
                    count = stoppage.TotalCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while fetching Stoppage Report data.",
                    error = ex.Message
                });
            }

        }

        [HttpGet("GetIdlingStatusReport")]
        public async Task<IActionResult> GetIdlingStatusReport([FromQuery] DataTableRequestModel dtmodel)
        {
            try
            {
                var stoppage = await _reportsService.GetIdlingStatusReport(dtmodel);
                if (dtmodel.DownloadType == "Excel")
                {
                    var reportName = $"IdlingStatus_{dtmodel.CustId}.xlsx";
                    var stream = await _importExportExcelService.ExportToExcelFlatList(stoppage.data, reportName, null, null);
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", reportName);
                }
                if (dtmodel.DownloadType == "Pdf")
                {
                    var reportName = $"IdlingStatus_{dtmodel.CustId}.pdf";
                    var stream = await _importExportPdfService.ExportToPdfFlatList(stoppage.data, reportName, null, null);
                    return File(stream, "application/pdf", reportName);
                }
                return Ok(new
                {
                    data = stoppage.data,
                    count = stoppage.TotalCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while fetching Idling data.",
                    error = ex.Message
                });
            }

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
                if (requestModel.DownloadType == "Excel")
                {
                    var reportName = $"OverSpeedAnalysis_{requestModel.beginDate:yyyyMMdd}_to_{requestModel.endDate:yyyyMMdd}_{DateTime.Now:HHmmss}.xlsx";
                    var stream = await _importExportExcelService.ExportToExcelFlatList(speedData.OSmainLst, reportName, null, null);
                    Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");
                    Response.Headers["Content-Disposition"] = $"attachment; filename={reportName}";
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", reportName);
                }
                if (requestModel.DownloadType == "Pdf")
                {
                    var reportName = $"OverSpeedAnalysis_{requestModel.beginDate:yyyyMMdd}_to_{requestModel.endDate:yyyyMMdd}_{DateTime.Now:HHmmss}.pdf";
                    var stream = await _importExportPdfService.ExportToPdfFlatList(speedData.OSmainLst, reportName, null, null);
                    Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");
                    Response.Headers["Content-Disposition"] = $"attachment; filename={reportName}";
                    return File(stream, "application/pdf", reportName);
                }
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

        [HttpGet("GetEntryExitReport")]


        public async Task<IActionResult> GetEntryExitReport([FromQuery] DataTableRequestModel model, string rtype, [FromQuery] string bbid = "")
        {
            var stoppage = await _reportsService.GetListofEntryExit(model, rtype, bbid);

            // Clean POIName
            stoppage.vehicleList?
                .SelectMany(v => v.poisCoveredList ?? Enumerable.Empty<POIEntryExitModel>())
                .Where(p => !string.IsNullOrWhiteSpace(p.POIName))
                .ToList()
                .ForEach(p => p.POIName = WebUtility.HtmlDecode(
                    Regex.Replace(p.POIName, "<.*?>", string.Empty)
                ).Trim());

            // Clean Duration
            stoppage.vehicleList?
                .SelectMany(v => v.poisCoveredList ?? Enumerable.Empty<POIEntryExitModel>())
                .Where(p => !string.IsNullOrWhiteSpace(p.duration))
                .ToList()
                .ForEach(p => p.duration = WebUtility.HtmlDecode(
                    Regex.Replace(p.duration, "<.*?>", string.Empty)
                ).Trim());

            if (model.DownloadType == "Excel")
            {
                var fileName = $"GetEntryExitReport_{model.beginDate:yyyyMMdd}_to_{model.endDate:yyyyMMdd}_{DateTime.Now:HHmmss}.xlsx";
                var stream = await _importExportExcelService.ExportToExcelFlatList(stoppage.vehicleList, fileName, null, null);
                Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");
                Response.Headers["Content-Disposition"] = $"attachment; filename={fileName}";

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            if (model.DownloadType == "Pdf")
            {
                var fileName = $"GetEntryExitReport_{model.beginDate:yyyyMMdd}_to_{model.endDate:yyyyMMdd}_{DateTime.Now:HHmmss}.pdf";
                var stream = await _importExportPdfService.ExportToPdfFlatList(stoppage.vehicleList, fileName, null, null);
                Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");
                Response.Headers["Content-Disposition"] = $"attachment; filename={fileName}";
                return File(stream, "application/pdf", fileName);
            }

            return Ok(new
            {
                data = stoppage.vehicleList,
                count = stoppage.PageCount
            });
        }

        [HttpGet("GetLiveStatus")]
        public async Task<IActionResult> GetLiveStatus(string pagename, [FromQuery] DataTableRequestModel model)
        {
            try
            {
                int sEcho = model.sEcho;
                int start = model.iDisplayStart;
                int length = model.iDisplayLength;
                string search = model.sSearch;
                string sortColumn = model.sortColumn;
                string sortDirection = model.sortDirection;


                var vehiclestatuslist = await _mongoService.GetLiveStatus(pagename, model);
                return Ok(new
                {
                    success = true,
                    message = "Vehicle data retrieved successfully",
                    data = vehiclestatuslist,

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
