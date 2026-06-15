using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trackmaster_Model;
using Trackmaster_Repository.Interface;
using Trackmaster_Service.Interface;
using static Trackmaster_Model.MongoModel;
using static Trackmaster_Model.Reports;
using static Trackmaster_Model.VehicleStatusModel;

namespace Trackmaster_Service.Service
{
    public class MongoService : IMongoService
    {
        private readonly IMongoRepository _mongoRepository;

        public MongoService(IMongoRepository mongoRepository)
        {
            _mongoRepository = mongoRepository;
        }

        //public async Task<string> GetSampleDocumentAsync()
        //{
        //    return await _mongoRepository.GetSampleDocumentAsync();
        //}
        //public async Task<List<DeviceLiveData>> GetLiveDataByBbids(List<string> bbids)
        //{
        //    return await _mongoRepository.GetLiveDataByBbids(bbids);
        //}

        //public async Task<string> GetLiveDataByBbids(List<string> bbids)
        //{
        //    return await _mongoRepository.GetLiveDataByBbids(bbids);
        //}
        public async Task<List<VehicleMaster>> GetLiveStatus(string pageName, DataTableRequestModel model)
        {
            var vehicles =  await _mongoRepository.GetLiveStatus(pageName, model);
            return vehicles;
        }
    }
}
