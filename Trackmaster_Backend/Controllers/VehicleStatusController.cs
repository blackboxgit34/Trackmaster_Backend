using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using Trackmaster_Model;
using Trackmaster_Service;
using Trackmaster_Service.Interface;
using static Trackmaster_Model.VehicleStatusModel;
namespace Trackmaster_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleStatusController : ControllerBase
    {
        private readonly ImportExportExcelService _importExportExcelService;
        private readonly IVehicleStatusService _vehiclestatusService;
        public VehicleStatusController(IVehicleStatusService vehiclestatusService, ImportExportExcelService importExportExcelService)
        {
            _vehiclestatusService = vehiclestatusService;
            _importExportExcelService = importExportExcelService;
        }

        [HttpGet("GetvehicleStatusList")]
        public async Task<IActionResult> GetvehicleStatusList(string pagename, [FromQuery] DataTableRequestModel model)
        {
            try
            {
                int sEcho = model.sEcho;
                int start = model.iDisplayStart;
                int length = model.iDisplayLength;
                string search = model.sSearch;
                string sortColumn = model.sortColumn;
                string sortDirection = model.sortDirection;


                var vehiclestatuslist = await _vehiclestatusService.GetvehicleStatusList(pagename, model);
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
        [HttpGet("GetPlaybackData")]
        public async Task<IActionResult> GetPlaybackData(string bbid, DateTime date, string downloadType = null)
        {
            try
            {
                var data = await _vehiclestatusService.GetPlaybackData(bbid, date);
                var playbackData = data.playbackData;
                var latLongData = data.latLongData;

                var movingData = new List<PlaybackDataModel>();

                if (playbackData == null || playbackData.Count == 0)
                {
                    return Ok(new
                    {
                        success = true,
                        data = movingData
                    });
                }

                if (!String.IsNullOrEmpty(downloadType) && (downloadType.Equals("Excel")))
                {
                    var reportName = $"RoutePlayback_{bbid}_{date:yyyyMMdd}.xlsx";

                    var stream = await _importExportExcelService.ExportToExcelFlatList(
                        playbackData,
                        reportName,
                        null,
                        null
                    );

                    return File(
                        stream,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        reportName
                    );
                }

                // =========================
                // FIRST MOVING POINT
                // =========================
                int startIndex = playbackData.FindIndex(x => x.speed > 0);

                if (startIndex == -1)
                {
                    return Ok(new
                    {
                        success = true,
                        data = movingData
                    });
                }

                // =========================
                // LAST MOVING POINT
                // =========================
                int lastMovingIndex = -1;

                for (int i = playbackData.Count - 1; i >= 0; i--)
                {
                    if (playbackData[i].speed > 0)
                    {
                        lastMovingIndex = i;
                        break;
                    }
                }

                // =========================
                // FIRST STOP AFTER LAST MOVING
                // =========================
                int endIndex = lastMovingIndex;

                for (int i = lastMovingIndex + 1; i < playbackData.Count; i++)
                {
                    if (playbackData[i].speed == 0)
                    {
                        endIndex = i;
                        break;
                    }
                }

                // =========================
                // PUSH START RECORD
                // =========================
                movingData.Add(playbackData[startIndex]);

                int? stopStartIndex = null;

                // =========================
                // LOOP
                // =========================
                for (int i = startIndex + 1; i < endIndex; i++)
                {
                    var current = playbackData[i];
                    var previous = playbackData[i - 1];

                    // =====================
                    // STOP START
                    // =====================
                    if (current.speed == 0 && previous.speed > 0)
                    {
                        stopStartIndex = i;
                    }

                    // =====================
                    // STOP END
                    // =====================
                    if (
                        stopStartIndex != null &&
                        (
                            current.speed > 0 ||
                            i == endIndex - 1
                        )
                    )
                    {
                        var stopStartPoint = playbackData[stopStartIndex.Value];
                        var stopEndPoint = playbackData[i];

                        var stopDuration =
                            (stopEndPoint.datadate - stopStartPoint.datadate)
                            .TotalMinutes;

                        // ONLY PUSH ONE STOP RECORD IF > 1 MIN
                        if (stopDuration > 1)
                        {
                            movingData.Add(stopStartPoint);
                        }

                        stopStartIndex = null;
                    }

                    // =====================
                    // MOVING RECORD
                    // =====================
                    if (current.speed > 0)
                    {
                        movingData.Add(current);
                    }
                }

                // =========================
                // PUSH END RECORD
                // =========================
                movingData.Add(playbackData[endIndex]);

                return Ok(new
                {
                    success = true,
                    message = "Vehicle data retrieved successfully",
                    data = playbackData,
                    movingData = movingData,
                    latLongData = latLongData
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


        [HttpPost("GetFuelLevels")]
        public async Task<IActionResult> GetFuelLevels([FromBody] FuelLevelRequestModel request)
        {
            try
            {
                var fuelLevels =await _vehiclestatusService.GetFuelLevels(request.BBIDs);
                return Ok(new
                {
                    success = true,
                    data = fuelLevels
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}
