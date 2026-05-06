using Trackmaster_Model;

namespace Trackmaster_Service.Interface
{
    public interface IDashboardService
    {
        Task<DashboardData> GetDashboardData(int userid, string type, string bbid);
       Task<List<VehicleList>> GetAllVehicleListByCustId(int custId);

    }
}
