using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trackmaster_Model
{
    public class DistanceReportDataModel
    {
        public string BBID { get; set; }
        public string VehName { get; set; }
        public string Distance { get; set; }
        public List<DistanceReportSubDataModel> _distanceReportSubDataModel { get; set; } = new List<DistanceReportSubDataModel>();
    }
    public class DistanceReportSubDataModel
    {
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Duration { get; set; }
        public string EstimateDistance { get; set; }
        public string EstimateCumulativeDistance { get; set; }
        public string StartLocation { get; set; }
    }
}
