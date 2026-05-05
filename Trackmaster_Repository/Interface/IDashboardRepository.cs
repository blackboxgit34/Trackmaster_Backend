using Trackmaster_Model;

namespace Trackmaster_Repository.Interface
{
    public interface IDashboardRepository
    {
        //DashboardData GetDashboardData(int userid);
        Task<VehicleStatus> GetVehicleStatus(int userid);
        Task<VehicleUtilization> GetVehicleUtilization(int userid);
        Task<SpeedAnalysis> GetSpeedAnalysis(int userid);
    }
}
