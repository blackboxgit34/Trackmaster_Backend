using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trackmaster_Model;
using static Trackmaster_Model.VehicleStatusModel;

namespace Trackmaster_Repository.Interface
{
    public interface IVehicleStatusRepository
    {
        Task<List<VehicleonMapList>> GetvehicleStatusList(string pagename, DataTableRequestModel model);
        Task<List<PlaybackDataModel>> GetPlaybackData(string bbid, DateTime date);
    }
}
