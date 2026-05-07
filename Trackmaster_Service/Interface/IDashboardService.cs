using Trackmaster_Model;

namespace Trackmaster_Service.Interface
{
    public interface IDashboardService
    {
        Task<DashboardData> GetDashboardData(int userid, DateTime start, DateTime end);
        List<VehicleList> GetAllVehicleListByCustId(int custId);
        OverSpeedReport GetOverSpeedGraphReport(int custid, string bbid);
    }
}
