using System.Collections.Generic;
using System.Threading.Tasks;
using Trackmaster_Model;

namespace Trackmaster_Service.Interface
{
    public interface IGeofenceService
    {
        Task<string> SaveGeofence(GeofenceModel model);
        Task<bool> LocationExist(float lat, float longi, int custid);
        Task<Boolean> SavePOI(double lat, double longi, int custid, string location, string radius);
        Task<List<PoiList>> GetPOi(string CustId);
        Task<ManagePoiResponse> ManagePoi(DataTableRequestModel request, string? id);
        Task<(List<GeofenceModel> geofenceList, int TotalCount)> GetGeofenceList(DataTableRequestModel model);
    }
}
