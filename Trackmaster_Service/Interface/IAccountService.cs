using Trackmaster_Model;

namespace Trackmaster_Service.Interface
{
    public interface IAccountService
    {
        LoginUser AuthorizeUser(string userId, string password, string type);
        List<MasterList> GetUserBySearching(string search);
        UserOtp VerifyUserOtp(int custid, string website, string OTP);
        string UpdateOTPAdminPassword(string custId, string NewPassword);
    }
}
