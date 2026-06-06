using System;
using System.Collections.Generic;
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
        public string id { get; set; }
        public string lat { get; set; }
        public string longi { get; set; }
        public string location { get; set; }
        public string radius { get; set; }=null;
        public string details { get; set; }
        public string StandardDistance { get; set; }
        public string poitype { get; set; }
    }


}
