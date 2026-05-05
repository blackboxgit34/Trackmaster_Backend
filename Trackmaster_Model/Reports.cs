using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trackmaster_Model
{
    public class Reports
    {
        public class VehiclesReport
        {
            public int PageCount { get; set; }
            public List<VehicleInformation> modelObjList { get; set; }
        }
        public class VehicleInformation 
        {
            public string VehicleName { get; set; }
            public string driverName { get; set; }
            public string ConductorName { get; set; }
            public string VehicleType { get; set; }
            public string VehicleImagePath { get; set; }
            public int TotalRecords { get; set; }
            public string BBID { get; set; }

        }
    }
}
