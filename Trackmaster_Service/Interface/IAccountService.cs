using HMSCL.Models;
using Trackmaster_Model;

namespace Trackmaster_Service.Interface
{
    public interface IAccountService
    {
        LoginUser AuthorizeUser(string userId, string password, string type);
        List<MasterList> GetUserBySearching(string search);
    }
}
