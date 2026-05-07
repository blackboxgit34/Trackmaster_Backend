using Microsoft.Extensions.Caching.Memory;
using System;
using Trackmaster_Model;
using Trackmaster_Repository.Interface;
using Trackmaster_Service.Interface;
namespace Trackmaster_Service.Service
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepository;
        private readonly IMemoryCache _cache;

        public DashboardService(IDashboardRepository dashboardRepository, IMemoryCache cache)
        {
            _dashboardRepository = dashboardRepository;
            _cache = cache;
        }

        public async Task<DashboardData> GetDashboardData(int userid, string type, string bbid, DateTime start, DateTime end)
        {
            string cacheKey = $"dashboard_{userid}_{type}_{bbid}_{start:yyyyMMddHHmm}_{end:yyyyMMddHHmm}";

            if (_cache.TryGetValue(cacheKey, out DashboardData cached))
            {
                return cached;
            }

            var dashboard = new DashboardData();

            try
            {
                switch (type?.ToLower())
                {
                    case "vehiclestatus":
                        dashboard.vehicleStatus = await _dashboardRepository.GetVehicleStatus(userid);
                        break;
                    case "speedanalysis":
                        dashboard.speedAnalysis = await _dashboardRepository.GetSpeedAnalysis(userid, start, end);
                        break;

                    case "avgspeedgraph":
                        dashboard.overSpeedReport = await _dashboardRepository.GetOverSpeedGraphData(userid,bbid);
                        break;

                    case null:
                        var vehicleStatusTask = _dashboardRepository.GetVehicleStatus(userid);
                        var utilizationTask = _dashboardRepository.GetVehicleUtilization(userid);
                        var speedTask = _dashboardRepository.GetSpeedAnalysis(userid, start, end);
                        var distanceTask = _dashboardRepository.GetDistanceDash(userid, start, end);
                        var graphData = _dashboardRepository.GetOverSpeedGraphData(userid, bbid);
                        var IdlingTask = _dashboardRepository.GetIdlingDuration(userid);

                        await Task.WhenAll(vehicleStatusTask, utilizationTask, speedTask, distanceTask, IdlingTask, graphData);

                        dashboard.vehicleStatus = vehicleStatusTask.Result;
                        dashboard.vehicleUtilization = utilizationTask.Result;
                        dashboard.speedAnalysis = speedTask.Result;
                        dashboard.distanceData = distanceTask.Result;
                        dashboard.overSpeedReport = graphData.Result;
                        dashboard.IdlingDuration = IdlingTask.Result;
                        break;
                }

                dashboard.IsSuccess = true;
                dashboard.Message = "Success";

                _cache.Set(cacheKey, dashboard, TimeSpan.FromMinutes(1));

                return dashboard;
            }
            catch (Exception ex)
            {
                return new DashboardData
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<List<VehicleList>> GetAllVehicleListByCustId(int userid)
        {
            return await _dashboardRepository.GetAllVehicleListByCustId(userid);
        }


    }
}
