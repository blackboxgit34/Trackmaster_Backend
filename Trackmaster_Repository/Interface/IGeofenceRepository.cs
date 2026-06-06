using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trackmaster_Model;

namespace Trackmaster_Repository.Interface
{
    public interface IGeofenceRepository
    {
        Task<string> SaveGeofence(GeofenceModel model);
    }
}
