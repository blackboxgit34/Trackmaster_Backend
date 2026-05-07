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
        public List<VehicleList> vehicleList { get; set; } = new List<VehicleList>();
        public List<OverSpeedReport> overSpeedReport { get; set; } = new List<OverSpeedReport>();
        public List<DistanceDashModel> distanceData { get; set; } = new List<DistanceDashModel>();
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

    public class VehicleList
    {
        public string VehName { get; set; }
        public string BBID { get; set; }

    }

    public class OverSpeedReport
    {
        public int overspeedCount { get; set; }
        public int OverCustomCount { get; set; }
        public string DateTime { get; set; }
    }
    public class DistanceDashModel
    {
        public string BBID { get; set; }
        public string VehicleName { get; set; }
        public double Distance { get; set; }
    }
}
