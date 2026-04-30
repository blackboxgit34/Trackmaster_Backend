using HMSCL.Models;

namespace Trackmaster_Service.Interface
{
    public interface IAccountService
    {
        LoginUser AuthorizeUser(string userId, string password);
    }
}
