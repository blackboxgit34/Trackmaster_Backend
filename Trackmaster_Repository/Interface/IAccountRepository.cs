using HMSCL.Models;
using Trackmaster_Model;

namespace Trackmaster_Repository.Interface
{
    public interface IAccountRepository
    {
        LoginUser AuthorizeUser(string userId, string password, string type);
        List<MasterList> GetUserBySearching(string search);
    }
}
