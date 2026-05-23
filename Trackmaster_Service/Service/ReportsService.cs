using HMSCL.Models;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trackmaster_Model;
using Trackmaster_Repository.Interface;
using Trackmaster_Repository.Repository;
using Trackmaster_Service.Interface;
using static Trackmaster_Model.Reports;

namespace Trackmaster_Service.Service
{
    public class ReportsService : IReportsService
    {
        private readonly IReportsRepository _reportsRepository;
        private readonly IMemoryCache _cache;
        public ReportsService(IReportsRepository reportsRepository, IMemoryCache cache)
        {
            _reportsRepository = reportsRepository;
            _cache = cache;
        }
        public async Task<VehiclesReport> GetConductorInfo(DataTableRequestModel requestModel)
        {
            return await _reportsRepository.GetConductorInfo(requestModel);

        }
        public async Task<List<DropDownItems>> GetDesignationTypeCrew()
        {
            return await _reportsRepository.GetDesignationTypeCrew();
        }
        public async Task<List<DropDownItems>> GetStatesList()
        {
            return await _reportsRepository.GetStatesList();
        }
        public async Task<List<DropDownItems>> GetCityList(int stateid)
        {
            return await _reportsRepository.GetCityList(stateid);
        }

        public async Task<string> AddUpdateEmployee(Employee objEmp, string imagePaths = "")
        {
            return await _reportsRepository.AddUpdateEmployee(objEmp, imagePaths);
        }
        public async Task<List<DropDownItems>> GetMessageType()
        {
            string cacheKey = "messageTypeList";
            if (_cache.TryGetValue(cacheKey, out List<DropDownItems> cached))
            {
                return cached;
            }
            var vehiclelist = await _reportsRepository.GetMessageType();
            _cache.Set(vehiclelist, TimeSpan.FromMinutes(10));
            return vehiclelist;
        }
        public async Task<SMSReportEx> GetSentMessagesReport(DataTableRequestModel requestModel, int typeid, string messagetype) // neha k 
        {
            return await _reportsRepository.GetSentMessagesReport(requestModel, typeid, messagetype);

        }



        public async Task<(List<StoppageSubModel> data, int TotalCount)> GetCombinedStoppageReport(DataTableRequestModel dtmodel)
        {
            return await _reportsRepository.GetCombinedStoppageReport(dtmodel);
        }

        public VehicleStatusResponse VehicleStatus(int custId, int lower, int upper, string search, DateTime start, DateTime end)
        {
            return _reportsRepository.VehicleStatus(custId, lower, upper, search, start, end);
        }
        public async Task<(List<DistanceReportDataModel> data, int TotalCount)> GetDistanceReportData(DataTableRequestModel model)
        {
            return await _reportsRepository.GetDistanceReportData(model);
        }
        public async Task<(List<DistanceMonthlyReportDataModel> data, int TotalCount)> GetMonthlyDistanceReportData(DataTableRequestModel model)
        {
            return await _reportsRepository.GetMonthlyDistanceReportData(model);
        }
       
    }
}
