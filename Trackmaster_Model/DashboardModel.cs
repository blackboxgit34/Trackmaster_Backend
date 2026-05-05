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
        public int IgnitionON { get; set; }
        public int Parked { get; set; }
        public int Totalvehicle { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
}
