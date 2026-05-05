using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trackmaster_Model
{
    
    public class DashboardData
    {
        public int TotalVehicles { get; set; }
        public int Moving { get; set; }
        public int Parked { get; set; }
        public int IgnitionOn { get; set; }
        public int HighSpeed { get; set; }
        public int Towed { get; set; }
        public int Unreachable { get; set; }
        public int BatteryDisconnect { get; set; }
        public int Breakdown { get; set; }

        // ✅ Important
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }

}
