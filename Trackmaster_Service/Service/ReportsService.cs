using HMSCL.Models;
using System;
using System.Collections.Generic;
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
        public VehiclesReport GetConductorInfo(int CustId,int lowerBound, int upperBound, string sSearch)
        {
            return _reportsRepository.GetConductorInfo( CustId, lowerBound,upperBound, sSearch);
        }
    }
}
