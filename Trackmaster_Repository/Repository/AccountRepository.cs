using HMSCL.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using Trackmaster_Model;
using Trackmaster_Repository.Interface;
using static Trackmaster_Repository.DataTypeHelper;
namespace Trackmaster_Repository.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private readonly string _connectionString43;
        public AccountRepository(IConfiguration configuration)
        {
            _connectionString43 = configuration.GetConnectionString("DefaultConnection43");
        }
        public LoginUser AuthorizeUser(string userId, string password, string type)
        {
            var objUser = new LoginUser();

            try
            {

                using (SqlConnection con = new SqlConnection(_connectionString43))
                {
                    con.Open();

                    using (SqlCommand cmd = new SqlCommand("SELECT CASE WHEN EXISTS (SELECT 1 FROM tbemp WHERE empId = @empId and password=@password and IsActive = 1) THEN 1 ELSE 0 END", con))
                    {
                        cmd.Parameters.AddWithValue("@empId", userId);
                        cmd.Parameters.AddWithValue("@password", password);
                        objUser.IsStaffMember = Convert.ToInt32(cmd.ExecuteScalar()) == 1;
                        objUser.IsSuccess = true;
                        objUser.Message = "User is from Staff !";
                    }
                    if (!objUser.IsStaffMember)
                    {
                        password = type == "Customer" ? EncryptPassword(password) : password;
                        using (SqlCommand cmd = new SqlCommand("[dbo].[ht_selcustnewtest]", con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@login", userId);
                            cmd.Parameters.AddWithValue("@pwd", password);

                            using (SqlDataReader dr = cmd.ExecuteReader())
                            {
                                if (!dr.HasRows)
                                {
                                    return new LoginUser
                                    {
                                        IsSuccess = false,
                                        Message = "Invalid Username or Password!"
                                    };
                                }

                                dr.Read();

                                int isBlocked = Convert.IsDBNull(dr["isblocked"]) ? 0 : Convert.ToInt32(dr["isblocked"]);
                                int custStatus = Convert.IsDBNull(dr["cust_status"]) ? 0 : Convert.ToInt32(dr["cust_status"]);

                                if (isBlocked == 1)
                                {
                                    return new LoginUser
                                    {
                                        IsSuccess = false,
                                        Message = "Your Account is blocked due to non payment!"
                                    };
                                }

                                if (custStatus == 0)
                                {
                                    return new LoginUser
                                    {
                                        IsSuccess = false,
                                        Message = "Your Account is deactivated!"
                                    };
                                }

                                objUser.CustId = GetString(dr["custid"]);
                                objUser.CustName = GetString(dr["name"]);
                                objUser.UserName = GetString(dr["login"]);
                                objUser.CustType = GetString(dr["type"]);
                                objUser.role = GetString(dr["role"]);
                                objUser.IsPasswordUpdated = GetBool(dr["IsPasswordUpdated"]);
                                objUser.IsCustomMenu = GetBool(dr["IsCustomMenu"]);
                                objUser.IsCustomDashboard = GetInt(dr["IsCustomDashboard"]);
                                objUser.IsCamEnable = GetBool(dr["IsCameraEnabled"]);
                                objUser.ForeignRoleIdFk = GetShort(dr["ForeignRoleIdFk"]);
                                objUser.ChannelPartnerID = GetInt(dr["ChannelPartnerID"]);
                                objUser.userTypeFlag = GetBool(dr["IsBlocked"]);

                                objUser.IsSuccess = true;
                                objUser.Message = "Login successful";

                                dr.Close();

                                using (SqlCommand cmdLoginLimit = new SqlCommand("[dbo].[CheckLoginLimit]", con))
                                {
                                    cmdLoginLimit.CommandType = CommandType.StoredProcedure;
                                    cmdLoginLimit.Parameters.AddWithValue("@custId", objUser.CustId);

                                    var result = cmdLoginLimit.ExecuteScalar();
                                    objUser.LoginCount = result != null ? Convert.ToInt32(result) : 0;
                                }

                                using (SqlCommand cmdInsertLogin = new SqlCommand("[dbo].[InsertLoginCustId]", con))
                                {
                                    cmdInsertLogin.CommandType = CommandType.StoredProcedure;
                                    cmdInsertLogin.Parameters.AddWithValue("@custId", objUser.CustId);
                                    cmdInsertLogin.ExecuteNonQuery();
                                }
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                return new LoginUser
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }

            return objUser;
        }
        public string EncryptPassword(string password)
        {
            SHA1 sha = new SHA1CryptoServiceProvider();
            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] combined = encoder.GetBytes(password);
            string encryptedPwd = BitConverter.ToString(sha.ComputeHash(combined)).Replace("-", "");
            return encryptedPwd;
        }
        public List<MasterList> GetUserBySearching(string search)
        {
            var userList = new List<MasterList>();

            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString43))
                {
                    con.Open();

                    string query = @"
                SELECT 
                    custid AS Id, 
                    [login] AS value, 
                    CONCAT([name], ' - ', custid) AS label,
                    pwd as otherValue 
                FROM ht_cust 
                WHERE 
                    ([name] LIKE @search OR custid LIKE @search OR [login] LIKE @search)
                    AND Cust_status = 1 
                    AND ISNULL(IsBlocked, 0) <> 1
                    ORDER BY custid";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@search", "%" + search + "%");
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                var objUser = new MasterList();
                                objUser.Id = GetInt(dr["Id"]);  
                                objUser.value = GetString(dr["value"]);
                                objUser.label = GetString(dr["label"]);
                                objUser.otherValue = GetString(dr["otherValue"]);
                                userList.Add(objUser);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return userList;
        }
    }
}
