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
        VehiclesReport GetConductorInfo(int CustId, int sEcho, int iDisplayStart, int iDisplayLength, string sSearch, string sortColumn, string sortDirection);
        List<DropDownItems> GetDesignationTypeCrew();
        List<DropDownItems> GetStatesList();
        List<DropDownItems> GetCityList(int stateid);
        string AddUpdateEmployee(Employee objEmp);
    }
}
