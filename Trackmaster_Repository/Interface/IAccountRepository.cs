using HMSCL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trackmaster_Repository.Interface
{
    public interface IAccountRepository
    {
        LoginUser AuthorizeUser(string userId, string password);
    }
}
