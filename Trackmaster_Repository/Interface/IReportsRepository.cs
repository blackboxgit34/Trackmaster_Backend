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
        Task<List<DropDownItems>> GetMessageType();
        Task<SMSReportEx> GetSentMessagesReport(DataTableRequestModel requestModel, int typeid, string messagetype, string vehicleNo); // neha k
        Task<ConsolidatedIgnitionModel> GetConsolidatedIgnitionStatus(DataTableRequestModel requestModel, string bbid, string reportName); // neha k
        Task<VehicleStatusResponse> VehicleStatus(int custId, int lower, int upper, string search, DateTime start, DateTime end);
        Task<VehicleStatusResponse> BatteryDisconnection(int custId, int lower, int upper, string search, DateTime start, DateTime end);
        Task<(List<StoppageSubModel> data, int TotalCount)> GetCombinedStoppageReport(DataTableRequestModel dtmodel);
        Task<(List<IdlingMainModel> data, int TotalCount)> GetIdlingStatusReport(DataTableRequestModel dtmodel);
        Task<(List<DistanceReportDataModel> data, int TotalCount)> GetDistanceReportData(DataTableRequestModel model);
        #region Neha Vaid
        Task<OverSpeedModel> getSpeedReport(string mode, DataTableRequestModel requestModel);
        #endregion
        Task<(List<DistanceMonthlyReportDataModel> data, int TotalCount)> GetMonthlyDistanceReportData(DataTableRequestModel model);
        Task<EntryExitReport> GetListofEntryExit(DataTableRequestModel requestModel, string bbid);
    }
}
