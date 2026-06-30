using Trackmaster_Model;

namespace Trackmaster_Repository.Interface
{
    public interface IAccountRepository
    {
        LoginUser AuthorizeUser(string userId, string password, string type);
        List<MasterList> GetUserBySearching(string search);
        UserOtp VerifyUserOtp(int custid, string website, string OTP);
        string UpdateOTPAdminPassword(string custId, string NewPassword);
    }
}
