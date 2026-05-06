

using Trackmaster_Model;
using Trackmaster_Repository.Interface;
using Trackmaster_Service.Interface;

namespace Trackmaster_Service.Repository
{
    
    public class DashboardService: IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepository;
        public DashboardService(IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        public DashboardData GetDashboardData(int userid)
        {
            return _dashboardRepository.GetDashboardData(userid);
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
