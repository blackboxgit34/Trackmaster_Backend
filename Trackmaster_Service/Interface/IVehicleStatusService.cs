using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trackmaster_Model;
using static Trackmaster_Model.VehicleStatusModel;

namespace Trackmaster_Service.Interface
{
    public interface IVehicleStatusService
    {
        Task<List<VehicleonMapList>> GetvehicleStatusList(int custId, string pagename);
        Task<List<PlaybackDataModel>> GetPlaybackData(string bbid, DateTime date);
    }
}
