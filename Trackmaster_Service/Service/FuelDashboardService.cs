using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trackmaster_Model;
using Trackmaster_Repository.Interface;
using Trackmaster_Service.Interface;

namespace Trackmaster_Service.Service
{
   public  class FuelDashboardService :IFuelDashboardService
    {
        private readonly IFuelDashboardRepository _fueldashboardRepository;
        public FuelDashboardService(IFuelDashboardRepository fueldashboardRepository)
        {
            _fueldashboardRepository = fueldashboardRepository;
        }
        public async Task<FuelDashboardModel> GetCurrentFuelData(int custid)
        {

           var dashboard = new FuelDashboardModel();

            try
            {
                dashboard = await _fueldashboardRepository.GetCurrentFuelData(custid);
                return dashboard;
            }
            catch (Exception ex)
            {
                return new FuelDashboardModel
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

    }
}
