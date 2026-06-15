using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trackmaster_Model;
using static Trackmaster_Model.MongoModel;

namespace Trackmaster_Service.Interface
{
    public interface IMongoService
    {
        //Task<string> GetSampleDocumentAsync();
        //Task<List<DeviceLiveData>> GetLiveDataByBbids(List<string> bbids);
        //Task<List<DeviceLiveData>> GetLiveDataByBbids(List<string> bbids);
        Task<List<VehicleMaster>> GetLiveStatus(string pagename, DataTableRequestModel model);
    }
}

