using System.Collections.Generic;
using System.Threading.Tasks;
using Trackmaster_Model;
using Trackmaster_Repository.Interface;
using Trackmaster_Repository.Repository;
using Trackmaster_Service.Interface;
using static Trackmaster_Model.Reports;

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
   
        public async Task<Boolean> SavePOI(double lat, double longi, int custid, string location, string radius)
        {
            return await _geofenceRepository.SavePOI(lat, longi, custid, location, radius);
        }

        public async Task<bool> LocationExist(float lat, float longi, int custid)
        {
            return await _geofenceRepository.LocationExist(lat, longi, custid);
        }

        public async Task<List<PoiList>>GetPOi(string CustId)
        {
            return await _geofenceRepository.GetPOI(CustId);
        }

        public async Task<ManagePoiResponse> ManagePoi(DataTableRequestModel request, string? id)
        {
            return await _geofenceRepository.ManagePoi(request, id);
        }
        public async Task<(List<GeofenceModel> geofenceList, int TotalCount)> GetGeofenceList(DataTableRequestModel model)
        {
            return await _geofenceRepository.GetGeofenceList(model);
        }
        public async Task<bool> DeleteGeofence(int FenceId, string Type)
        {
            return await _geofenceRepository.DeleteGeofence(FenceId, Type);
        }

        public async Task<(List<GeoFenceViolation> Data, int TotalCount)> GetGeoFenceViolationReport(DataTableRequestModel requestModel, string bbid)
        {
            return await _geofenceRepository.GetGeoFenceViolationReport(requestModel, bbid);
        }


       
    }
}
