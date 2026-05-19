using HMSCL.Models;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trackmaster_Repository.Interface;
using Trackmaster_Repository.Repository;
using Trackmaster_Service.Interface;
using static Trackmaster_Model.Reports;

namespace Trackmaster_Service.Service
{
    public class ReportsService : IReportsService
    {
        private readonly IReportsRepository _reportsRepository;
        public ReportsService(IReportsRepository reportsRepository)
        {
            _reportsRepository = reportsRepository;
        }
        public VehiclesReport GetConductorInfo(int CustId, int sEcho, int iDisplayStart, int iDisplayLength, string sSearch, string sortColumn, string sortDirection)
        {
            return _reportsRepository.GetConductorInfo(CustId, sEcho,iDisplayStart, iDisplayLength, sSearch, sortColumn, sortDirection);

        }
        public List<DropDownItems> GetDesignationTypeCrew()
        {
            return _reportsRepository.GetDesignationTypeCrew();
        }
        public List<DropDownItems> GetStatesList()
        {
            return _reportsRepository.GetStatesList();
        }
        public List<DropDownItems> GetCityList(int stateid)
        {
            return _reportsRepository.GetCityList(stateid);
        }

        public  string AddUpdateEmployee(Employee objEmp)
        {
            return _reportsRepository.AddUpdateEmployee(objEmp);
        }
        public StoppageMainModel GetCombinedStoppageReport(DateTime beginDate, DateTime endDate,string interval,  int custid, int lowerBound, int upperBound, string searchText)
        {
            return _reportsRepository.GetCombinedStoppageReport(beginDate, endDate, interval, custid, lowerBound, upperBound, searchText);
        }

    }
}
