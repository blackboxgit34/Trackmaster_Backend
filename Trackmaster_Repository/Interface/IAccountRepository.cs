using HMSCL.Models;

namespace Trackmaster_Repository.Interface
{
    public interface IAccountRepository
    {
        LoginUser AuthorizeUser(string userId, string password);
    }
}
