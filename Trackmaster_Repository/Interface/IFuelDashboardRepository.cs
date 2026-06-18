using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trackmaster_Model;

namespace Trackmaster_Repository.Interface
{
   public interface IFuelDashboardRepository
    {
        Task<FuelDashboardModel> GetCurrentFuelData(int custid);
         Task<List<FuelAnalysisResult>> FuelDisconAnalysisAsync(DateTime beginDate, DateTime endDate, string tblName, string analysisString);
    }
}
