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

        public async Task<DashboardData> GetDashboardData(int userid)
        {
            string cacheKey = $"dashboard_{userid}";

            if (_cache.TryGetValue(cacheKey, out DashboardData cached))
            {
                return cached;
            }

            var dashboard = new DashboardData();

            try
            {
                var vehicleStatusTask = _dashboardRepository.GetVehicleStatus(userid);
                var utilizationTask = _dashboardRepository.GetVehicleUtilization(userid);
                var speedTask = _dashboardRepository.GetSpeedAnalysis(userid);

                await Task.WhenAll(vehicleStatusTask, utilizationTask, speedTask);

                dashboard.vehicleStatus = vehicleStatusTask.Result;
                dashboard.vehicleUtilization = utilizationTask.Result;
                dashboard.speedAnalysis = speedTask.Result;

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



        public List<VehicleList> GetAllVehicleListByCustId(int custId)
        {
            return _dashboardRepository.GetAllVehicleListByCustId(custId);
        }

        public OverSpeedReport GetOverSpeedGraphReport(int custid,string bbid)
        {
            return _dashboardRepository.GetOverSpeedGraphReport(custid, bbid);
        }

    }
}
