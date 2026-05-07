using Trackmaster_Model;

namespace Trackmaster_Repository.Interface
{
    public interface IDashboardRepository
    {
        Task<VehicleStatus> GetVehicleStatus(int userid);
        Task<VehicleUtilization> GetVehicleUtilization(int userid);
        Task<SpeedAnalysis> GetSpeedAnalysis(int userid);
        Task<List<IdlingDuration>> GetIdlingDuration(int userid);
        List<VehicleList> GetAllVehicleListByCustId(int custId);
        OverSpeedReport GetOverSpeedGraphReport(int custid, string bbid);
    }
}
