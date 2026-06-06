using Trackmaster_Model;

namespace Trackmaster_Service.Interface
{
    public interface IGeofenceService
    {
        Task<string> SaveGeofence(GeofenceModel model);
    }
}
