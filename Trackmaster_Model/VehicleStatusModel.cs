using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trackmaster_Model
{
    public class VehicleStatusModel
    {
        public class VehicleonMapList
        {
            public string VehName { get; set; }
            public string Type { get; set; }
            public string VehicleStatus { get; set; }
            public string model { get; set; }
            //status: item.currstatus,
            public float lat { get; set; }
            public float lng { get; set; }
            public string speed { get; set; }
            public string location { get; set; }
            public string lastUpdated { get; set; }
            public string bbid { get; set; }
            public int gsmSignal { get; set; }
            public int gpsAntConStatus { get; set; }
            public int GPSFix { get; set; }
            public string IgnitionStatus { get; set; }
            public int vehBattery { get; set; }
            public int deviceBattery { get; set; }
            // ADD THIS
            public int TotalRecords { get; set; }
            public string driverName { get; set; }
            public string mob_no { get; set; }
        }


        public class GetFuelLevelsModel
        {
            public string BBID { get; set; }

            public decimal CurrentFuelLevel { get; set; }

            public decimal RemainingFuelLevel { get; set; }

            public decimal TotalFuel { get; set; }

            public string LastDateTime { get; set; }

            public string DisconnectedData { get; set; }
        }

        public class FuelLevelRequestModel
        {
            public List<string> BBIDs { get; set; }
        }
    }
}
