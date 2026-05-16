using HMSCL.Models;
using System;
using System.Collections.Generic;
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
        public ReportsService(IReportsRepository reportsRepository)
        {
            _reportsRepository = reportsRepository;
        }
        public VehiclesReport GetConductorInfo(DataTableRequestModel requestModel)
        {
            return _reportsRepository.GetConductorInfo(requestModel);

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

        //public  string AddUpdateEmployee(Employee objEmp)
        //{
        //    return _reportsRepository.AddUpdateEmployee(objEmp);
        //}

        public string AddUpdateEmployee(Employee objEmp, string imagePaths = "")
        {
            return _reportsRepository.AddUpdateEmployee(objEmp, imagePaths);
        }

    }
}
