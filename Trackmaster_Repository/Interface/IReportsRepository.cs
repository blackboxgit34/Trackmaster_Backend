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
        Task<VehiclesReport> GetConductorInfo(DataTableRequestModel requestModel);
        Task<List<DropDownItems>> GetDesignationTypeCrew();
        Task<List<DropDownItems>> GetStatesList();
        Task<List<DropDownItems>> GetCityList(int stateid);
        Task<string> AddUpdateEmployee(Employee objEmp, string imagePaths = "");
        VehicleStatusResponse VehicleStatus(int custId, int lower, int upper, string search, DateTime start, DateTime end);
        StoppageMainModel GetCombinedStoppageReport(DataTableRequestModel dtmodel);
        Task<List<DistanceReportDataModel>> GetDistanceReportData(DataTableRequestModel model);
        Task<List<DropDownItems>> GetMessageType();
    }
}
