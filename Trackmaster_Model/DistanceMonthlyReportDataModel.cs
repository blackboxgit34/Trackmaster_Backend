using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trackmaster_Model
{
    public class DistanceMonthlyReportDataModel
    {
        public string BBID { get; set; }
        public string VehName { get; set; }
        public string TotalDistance { get; set; }
        public string TotalStoppage { get; set; }
        public List<DistanceMonthlyReportSubDataModel> _distanceMonthlyReportSubDataModels { get; set; } = new List<DistanceMonthlyReportSubDataModel>();
    }
    public class DistanceMonthlyReportSubDataModel
    {
        public int Day { get; set; }
        public string Duration { get; set; }
        public string Distance { get; set; }
    }
}
