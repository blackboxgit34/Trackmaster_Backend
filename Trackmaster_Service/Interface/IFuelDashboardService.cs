using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trackmaster_Model;

namespace Trackmaster_Service.Interface
{
   public  interface IFuelDashboardService
    {
        Task<FuelDashboardModel> GetCurrentFuelData(int custid);
    }
}
