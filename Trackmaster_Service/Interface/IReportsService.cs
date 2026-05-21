using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Trackmaster_Model;
using static Trackmaster_Model.Reports;

namespace Trackmaster_Service.Interface
{
    public interface IReportsService
    {
        Task<VehiclesReport> GetConductorInfo(DataTableRequestModel requestModel);
        Task<List<DropDownItems>> GetDesignationTypeCrew();
        Task<List<DropDownItems>> GetStatesList();
        Task<List<DropDownItems>> GetCityList(int stateid);
        Task<string> AddUpdateEmployee(Employee objEmp, string imagePaths = "");
        Task<List<DropDownItems>> GetMessageType();
        Task<SMSReportEx> GetSentMessagesReport(DataTableRequestModel requestModel, int typeid, string messagetype); //neha k
        VehicleStatusResponse VehicleStatus(int custId, int lower, int upper, string search, DateTime start, DateTime end);
        StoppageMainModel GetCombinedStoppageReport(DataTableRequestModel DataTableRequestModel);
        Task<(List<DistanceReportDataModel> data, int TotalCount)> GetDistanceReportData(DataTableRequestModel model);
    }
}
