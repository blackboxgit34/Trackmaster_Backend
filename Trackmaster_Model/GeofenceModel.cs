using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trackmaster_Model
{
    public class GeofenceModel
    {
        public int FenceId { get; set; }
        public string FenceName { get; set; }
        public string Radius { get; set; }
        public string FenceType { get; set; }
        public List<VehicleList> vehicleLists { get; set; } = new List<VehicleList>();
        public List<LatLongHistory> latLongList { get; set; } = new List<LatLongHistory>();
    }

    public class AddPoiRequest
    {
        public string custid { get; set; }
        public string lat { get; set; }
        public string longi { get; set; }
        public string location { get; set; }
        public string radius { get; set; }=null;
    }


    public class PoiList
    {
        public string id { get; set; }
        public string lat { get; set; }
        public string lng { get; set; }
        public string details { get; set; }
        public string StandardDistance { get; set; }
        public string poitype { get; set; }

    }

    public class GeoFenceViolation
    {
        public int id { get; set; }
        public String VehicleName { get; set; }
        public String Location { get; set; }
        public String GeoTime { get; set; }
        public String FenceStatus { get; set; }
        public string BBID { get; set; }
        public int Speed { get; set; }
        public String fencename { get; set; }
        public int FenceViolationsCount { get; set; }
        public int PageCount { get; set; }
    }


}
