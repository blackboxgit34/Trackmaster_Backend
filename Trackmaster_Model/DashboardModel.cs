using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Trackmaster_Model
{

    public class DashboardData
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public VehicleStatus vehicleStatus { get; set; } = new VehicleStatus();
        public VehicleUtilization vehicleUtilization { get; set; } = new VehicleUtilization();
        public SpeedAnalysis speedAnalysis { get; set; } = new SpeedAnalysis();
    }
    public class VehicleStatus
    {
        public int TotalVehicles { get; set; }
        public int Moving { get; set; }
        public int HighSpeed { get; set; }
        public int IgnitionON { get; set; }
        public int Parked { get; set; }
        public int Towed { get; set; }
        public int Unreachable { get; set; }
        public int BatteryDisconnect { get; set; }
        public int Breakdown { get; set; }
    }
    public class VehicleUtilization
    {
        public int TotalVehicles { get; set; }
        public int Moving { get; set; }
        public int IgnitionON { get; set; }
        public int Parked { get; set; }
    }
    public class SpeedAnalysis
    {
        public int OS { get; set; }
        public int nonOS { get; set; }
    }
}


    public class VehicleList
    {
        public string VehName { get; set; }
        public string BBID { get; set; }

    }



    public class OverSpeedReport
    {
        public int PageCount { get; set; }
        public decimal OverspeedVehicles { get; set; }
        public decimal NonOverspeedVehicles { get; set; }
        public List<OverSpeedAnalysisEx> vehicleList { get; set; }

    }

    public class OverSpeedAnalysisEx
    {
        [DisplayName("Vehicle Name")]
        public string vehname { get; set; }
        [DisplayName("Driver Name")]
        public string driverName { get; set; }
        [DisplayName("Overspeed Count")]
        public int overspeedCount { get; set; }
        [DisplayName("Overspeed Limit")]
        public int overspeedLimit { get; set; }
        [DisplayName("Max Speed")]
        public int maxSpeed { get; set; }
        [DisplayName("Total Overspeed Driving Duration")]
        public string overSpeedDuration { get; set; }

        public int serialno { get; set; }
        public string bbid { get; set; }
        public int OverCustomCount { get; set; }

        public List<OverSpeedAnalysis> overSpeedData { get; set; }


        public string DateTime { get; set; } // new 

    }

    public class OverSpeedAnalysis
    {
        //private string dateTime;

        public string DateTime { get; set; }

        
        public string Location { get; set; }
        public string Speed { get; set; }

        public string status { get; set; }
    }
}
