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
        // ✅ Important
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }

}
