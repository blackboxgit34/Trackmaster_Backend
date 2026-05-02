

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
    }
}
