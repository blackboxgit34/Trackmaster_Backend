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


    public class FuelAnalysisResult
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Duration { get; set; }
        public long Duration1 { get; set; }
        public string SLoc { get; set; }
        public string SLat { get; set; }
        public string SLong { get; set; }
        public string ELoc { get; set; }
        public string ELat { get; set; }
        public string ELong { get; set; }
        // New properties
        public int DisconCount { get; set; }
        public int GarbageCount { get; set; }
    }


}
