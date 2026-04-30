using HMSCL.Models;
using Trackmaster_Repository.Interface;
using Trackmaster_Service.Interface;

namespace Trackmaster_Service.Repository
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        public AccountService(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }
        public LoginUser AuthorizeUser(string userId, string password)
        {
            return _accountRepository.AuthorizeUser(userId, password);
        }
    }
}
