using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trackmaster_Model;

namespace Trackmaster_Repository.Interface
{
    public interface IDashboardRepository
    {
        Task<VehicleStatus> GetVehicleStatus(int userid);
        Task<VehicleUtilization> GetVehicleUtilization(int userid);
        Task<SpeedAnalysis> GetSpeedAnalysis(int userid);
        Task<List<VehicleList>> GetAllVehicleListByCustId(int userid);
        Task<List<OverSpeedReport>> GetOverSpeedGraphData(int custid, string bbid);
        Task<List<DistanceDashModel>> GetDistanceDash(int custId, DateTime start, DateTime end);
    }
}
