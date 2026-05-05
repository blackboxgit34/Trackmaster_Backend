

using Trackmaster_Model;

namespace Trackmaster_Repository.Interface
{
    public interface IDashboardRepository
    {
        DashboardData GetDashboardData(int userid);
    }
}
