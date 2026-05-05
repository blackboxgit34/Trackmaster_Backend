using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Trackmaster_Model.Reports;

namespace Trackmaster_Service.Interface
{
    public interface IReportsService
    {
         VehiclesReport GetConductorInfo(int CustId,int lowerBound, int upperBound, string sSearch);
    }
}
