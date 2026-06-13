using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trackmaster_Model
{
   
        public class FuelDashboardModel
        {     
        public int totalGenset { get; set; }
        public int normalLevel { get; set; }
        public int lowLevel { get; set; }

        public string Message { get; set; }
        public bool IsSuccess { get; set; }
       
    }
    
}
