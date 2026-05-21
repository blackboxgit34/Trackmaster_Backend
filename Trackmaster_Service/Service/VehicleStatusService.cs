using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trackmaster_Model;
using Trackmaster_Repository.Interface;
using Trackmaster_Repository.Repository;
using Trackmaster_Service.Interface;
using static Trackmaster_Model.VehicleStatusModel;

namespace Trackmaster_Service.Service
{
    public class VehicleStatusService : IVehicleStatusService
    {
        private readonly IVehicleStatusRepository _vehicleStatusRepository;
        private readonly IMemoryCache _cache;

        public VehicleStatusService(IVehicleStatusRepository vehicleStatusRepository, IMemoryCache cache)
        {
            _vehicleStatusRepository = vehicleStatusRepository;
            _cache = cache;
        }
        public async Task<List<VehicleonMapList>> GetvehicleStatusList(string pagename, DataTableRequestModel model)
        {
            return await _vehicleStatusRepository.GetvehicleStatusList(pagename,model);
        }
        public async Task<List<PlaybackDataModel>> GetPlaybackData(string bbid, DateTime date)
        {
            return await _vehicleStatusRepository.GetPlaybackData(bbid, date);
        }
        public async Task<List<GetFuelLevelsModel>> GetFuelLevels(List<string> bbids)
        {
            return await _vehicleStatusRepository.GetFuelLevels(bbids);
        }
    }
}
