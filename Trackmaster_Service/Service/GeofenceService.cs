using Trackmaster_Model;
using Trackmaster_Repository.Interface;
using Trackmaster_Service.Interface;

namespace Trackmaster_Service.Service
{
    public class GeofenceService : IGeofenceService
    {
        private readonly IGeofenceRepository _geofenceRepository;
        public GeofenceService(IGeofenceRepository geofenceRepository)
        {
            _geofenceRepository = geofenceRepository;
        }
        public async Task<string> SaveGeofence(GeofenceModel model)
        {
            return await _geofenceRepository.SaveGeofence(model);
        }
        public async Task<(List<GeofenceModel> geofenceList, int TotalCount)> GetGeofenceList(DataTableRequestModel model)
        {
            return await _geofenceRepository.GetGeofenceList(model);
        }
    }
}
