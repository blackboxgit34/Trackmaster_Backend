using HMSCL.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trackmaster_Repository.Interface;
using System.Security.Cryptography;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
namespace Trackmaster_Repository.Repository
{
    public class AccountRepository:IAccountRepository
    {
        private readonly string _connectionString43;
        public AccountRepository(IConfiguration configuration)
        {
            _connectionString43 = configuration.GetConnectionString("DefaultConnection43");
        }
        public LoginUser AuthorizeUser(string userId, string password)
        {
            password = EncryptPassword(password);
            using (SqlConnection con = new SqlConnection(_connectionString43))
            {
                con.Open();

                LoginUser objUser = new LoginUser();

                // Prepare command for [dbo].[ht_selcustnewtest]
                using (SqlCommand cmd = new SqlCommand("[dbo].[ht_selcustnewtest]", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@login", userId);
                    cmd.Parameters.AddWithValue("@pwd", password);

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            dr.Read();

                            objUser.CustId = Convert.IsDBNull(dr["custid"]) ? string.Empty : Convert.ToString(dr["custid"]);
                            objUser.IsPasswordUpdated = Convert.IsDBNull(dr["IsPasswordUpdated"]) ? false : Convert.ToBoolean(dr["IsPasswordUpdated"]);

                            int isBlocked = Convert.IsDBNull(dr["isblocked"]) ? 0 : Convert.ToInt32(dr["isblocked"]);
                            int custStatus = Convert.IsDBNull(dr["cust_status"]) ? 0 : Convert.ToInt32(dr["cust_status"]);

                            if (isBlocked == 1 && custStatus == 1)
                            {
                                objUser.msg = "Your Account is blocked due to non payment! Kindly contact your Account executive.";
                                return objUser;
                            }
                            else if (isBlocked == 1 && custStatus == 0)
                            {
                                objUser.msg = "Your Account is blocked due to non payment! Kindly contact your Account executive.";
                                return objUser;
                            }
                            else if (isBlocked == 0 && custStatus == 0)
                            {
                                objUser.msg = "Your Account is deactive! Kindly contact your Service executive.";
                                return objUser;
                            }
                            else
                            {
                                // Read all remaining fields BEFORE closing the reader
                                objUser.CustName = Convert.IsDBNull(dr["name"]) ? string.Empty : Convert.ToString(dr["name"]);
                                objUser.IsCustomMenu = Convert.IsDBNull(dr["IsCustomMenu"]) ? false : Convert.ToBoolean(dr["IsCustomMenu"]);
                                objUser.IsCustomDashboard = Convert.IsDBNull(dr["IsCustomDashboard"]) ? 0 : Convert.ToInt32(dr["IsCustomDashboard"]);
                                objUser.CustType = Convert.IsDBNull(dr["type"]) ? string.Empty : Convert.ToString(dr["type"]);
                                objUser.UserName = Convert.IsDBNull(dr["login"]) ? string.Empty : Convert.ToString(dr["login"]);
                                objUser.userTypeFlag = Convert.IsDBNull(dr["IsBlocked"]) ? false : Convert.ToBoolean(dr["IsBlocked"]);
                                objUser.role = Convert.IsDBNull(dr["role"]) ? "0" : Convert.ToString(dr["role"]);
                                objUser.IsCamEnable = Convert.IsDBNull(dr["IsCameraEnabled"]) ? false : Convert.ToBoolean(dr["IsCameraEnabled"]);
                                objUser.ForeignRoleIdFk = Convert.IsDBNull(dr["ForeignRoleIdFk"]) ? (short)0 : Convert.ToInt16(dr["ForeignRoleIdFk"]);
                                objUser.ChannelPartnerID = Convert.IsDBNull(dr["ChannelPartnerID"]) ? 0 : Convert.ToInt32(dr["ChannelPartnerID"]);
                                objUser.msg = "Login";
                            }

                            // Close reader before executing other commands
                            dr.Close();

                            // Check login limit
                            using (SqlCommand cmdLoginLimit = new SqlCommand("[dbo].[CheckLoginLimit]", con))
                            {
                                cmdLoginLimit.CommandType = CommandType.StoredProcedure;
                                cmdLoginLimit.Parameters.AddWithValue("@custId", objUser.CustId);
                                object loginCountObj = cmdLoginLimit.ExecuteScalar();
                                objUser.LoginCount = loginCountObj != null ? Convert.ToInt32(loginCountObj) : 0;
                            }

                            // Insert login customer id for analysis
                            using (SqlCommand cmdInsertLogin = new SqlCommand("[dbo].[InsertLoginCustId]", con))
                            {
                                cmdInsertLogin.CommandType = CommandType.StoredProcedure;
                                cmdInsertLogin.Parameters.AddWithValue("@custId", objUser.CustId);
                                cmdInsertLogin.ExecuteScalar();
                            }
                        }
                    }
                }

                return objUser;
            }
        }
        public string EncryptPassword(string password)
        {
            SHA1 sha = new SHA1CryptoServiceProvider();
            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] combined = encoder.GetBytes(password);
            string encryptedPwd = BitConverter.ToString(sha.ComputeHash(combined)).Replace("-", "");
            return encryptedPwd;
        }
    }
}
