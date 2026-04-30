namespace HMSCL.Models
{
    public class LoginUser
    {
        public string CustId { get; set; }
        public string UserName { get; set; }
        public int IsCustomDashboard { get; set; }
        public string Password { get; set; }
        public string AdminPassword { get; set; }
        public string CustName { get; set; }
        public string CustType { get; set; }
        public string EnterSchm { get; set; }
        public string MainContent { get; set; }
        public string SubContent { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsPasswordUpdated { get; set; }
        public int SpecialRole { get; set; }
        public bool IsCamEnable { get; set; }
        public short? ForeignRoleIdFk { get; set; }
        public int? ChannelPartnerID { get; set; }
        public bool userTypeFlag { get; set; }
        public bool IsCustomMenu { get; set; }
        public string ParentId { get; set; }
        public int LoginCount { get; set; }
        public string error { get; set; }
        public string msg { get; set; }
        public string role { get; set; }
        public string Roles { get; set; }
        public string CustKey { get; set; }
        public string showIcons { get; set; }
        public string AppVersion { get; set; }
        public bool IsSubUser { get; set; }
        public bool Status { get; set; }
        public string Announcement { get; set; }
        public string ErrorCode { get; set; }
    }
}
