using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trackmaster_Model;

namespace Trackmaster_Repository.Interface
{
    public interface IGeofenceRepository
    {
        Task<string> SaveGeofence(GeofenceModel model);
        Task<bool> LocationExist(double lat, double longi, int custid);
        Task<Boolean> SavePOI(double lat, double longi, int custid, string location, string radius);
        Task<List<PoiList>> GetPOI(string custId);
        Task<(List<GeoFenceViolation> Data, int TotalCount)> GetGeoFenceViolationReport(DataTableRequestModel requestModel, string bbid);
    }
}
