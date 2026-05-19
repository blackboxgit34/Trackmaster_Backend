using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
