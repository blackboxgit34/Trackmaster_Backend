using Trackmaster_Model;

namespace Trackmaster_Service.Interface
{
    public interface IGeofenceService
    {
        Task<string> SaveGeofence(GeofenceModel model);
        Task<(List<GeofenceModel> geofenceList, int TotalCount)> GetGeofenceList(DataTableRequestModel model);
    }
}
