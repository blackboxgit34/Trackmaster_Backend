using Trackmaster_Model;

namespace Trackmaster_Service.Interface
{
    public interface IGeofenceService
    {
        Task<string> SaveGeofence(GeofenceModel model);
        Task<bool> LocationExist(float lat, float longi, int custid);
        Task<Boolean> SavePOI(double lat, double longi, int custid, string location, string radius);
        Task<List<PoiList>> GetPOi(string CustId);
        Task<(List<GeoFenceViolation> Data, int TotalCount)> GetGeoFenceViolationReport(DataTableRequestModel requestModel, string bbid);
    }
}
