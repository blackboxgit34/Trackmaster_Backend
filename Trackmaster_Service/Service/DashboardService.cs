using Trackmaster_Model;
using Trackmaster_Repository.Interface;
using Trackmaster_Service.Interface;
using Microsoft.Extensions.Caching.Memory;
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

        public async Task<DashboardData> GetDashboardData(int userid, string type, string bbid)
        {
            string cacheKey = $"dashboard_{userid}_{type}_{bbid}";

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

                    case null:
                        var vehicleStatusTask = _dashboardRepository.GetVehicleStatus(userid);
                        var utilizationTask = _dashboardRepository.GetVehicleUtilization(userid);
                        var speedTask = _dashboardRepository.GetSpeedAnalysis(userid);
                        //var graphData = _dashboardRepository.GetOverSpeedGraphData(userid, bbid);

                        await Task.WhenAll(vehicleStatusTask, utilizationTask, speedTask);

                        dashboard.vehicleStatus = vehicleStatusTask.Result;
                        dashboard.vehicleUtilization = utilizationTask.Result;
                        dashboard.speedAnalysis = speedTask.Result;
                        //dashboard.overSpeedReport = graphData.Result;
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
