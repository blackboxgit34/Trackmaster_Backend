using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trackmaster_Model;
using static Trackmaster_Model.Reports;

namespace Trackmaster_Repository.Interface
{
    public interface IReportsRepository
    {
         VehiclesReport GetConductorInfo(DataTableRequestModel requestModel);
        List<DropDownItems> GetDesignationTypeCrew();
        List<DropDownItems> GetStatesList();
        List<DropDownItems> GetCityList(int stateid);
        string AddUpdateEmployee(Employee objEmp, string imagePaths = "");
        VehicleStatusResponse VehicleStatus(int custId, int lower, int upper, string search, DateTime start, DateTime end);
        Task<List<DistanceReportDataModel>> GetDistanceReportData(DataTableRequestModel model);
    }
}
