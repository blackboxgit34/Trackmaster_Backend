namespace HMSCL.Models
{
    public class LoginUser
    {
        public string CustId { get; set; }
        public string CustName { get; set; }
        public string UserName { get; set; }
        public string CustType { get; set; }
        public bool IsPasswordUpdated { get; set; }
        public bool IsCustomMenu { get; set; }
        public int IsCustomDashboard { get; set; }
        public bool IsCamEnable { get; set; }
        public short ForeignRoleIdFk { get; set; }
        public int ChannelPartnerID { get; set; }
        public string role { get; set; }
        public bool userTypeFlag { get; set; }
        public int LoginCount { get; set; }

        // ✅ Important
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
}
