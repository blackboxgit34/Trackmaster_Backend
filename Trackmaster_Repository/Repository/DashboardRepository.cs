using HMSCL.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trackmaster_Model;
using Trackmaster_Repository.Interface;

namespace Trackmaster_Repository.Repository
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly string _connectionString43;
        public DashboardRepository(IConfiguration configuration)
        {
            _connectionString43 = configuration.GetConnectionString("DefaultConnection43");
        }
        public DashboardData GetDashboardData(int userid, string type)
        {
            var objUser = new DashboardData();
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString43))
                {
                    con.Open();
                    switch (type)
                    {
                        case "VehicleStatus":

                            using (SqlCommand cmd = new SqlCommand("getDashTotalvehicle", con))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@custid", userid);


                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        objUser.TotalVehicles = Convert.ToInt32(reader["VehicleCount"]);
                                        objUser.Moving = Convert.ToInt32(reader["Moving"]);
                                        objUser.HighSpeed = Convert.ToInt32(reader["HiSpeed"]);
                                        objUser.IgnitionOn = Convert.ToInt32(reader["IgnitionOn"]);
                                        objUser.Parked = Convert.ToInt32(reader["Parked"]);
                                        objUser.Towed = Convert.ToInt32(reader["Towed"]);
                                        objUser.Unreachable = Convert.ToInt32(reader["Unreachable"]);
                                        objUser.BatteryDisconnect = Convert.ToInt32(reader["BatteryDisconnect"]);
                                        objUser.Breakdown = Convert.ToInt32(reader["Breakdown"]);
                                    }
                                }
                            }
                            break;
                        case "VehicleUtilization":
                            using (SqlCommand cmd = new SqlCommand("GetVehicleUtilizationData", con))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@custid", userid);

                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        objUser.IgnitionON = Convert.ToInt32(reader["IgnitionON"]);
                                        objUser.Moving = Convert.ToInt32(reader["Moving"]);
                                        objUser.Parked = Convert.ToInt32(reader["Parked"]);
                                        objUser.Totalvehicle = Convert.ToInt32(reader["Totalvehicle"]);
                                    }
                                    objUser.IgnitionON = objUser.IgnitionON / 3600;
                                    objUser.Moving = objUser.Moving / 3600;
                                    objUser.Parked = objUser.Parked / 3600;
                                }
                            }
                            break;
                        default:
                            break;
                    }
                    con.Close();
                }
                objUser.IsSuccess = true;
                objUser.Message = "Dashboard data retrieved successfully";
                return objUser;
            }
            catch (Exception ex)
            {
                objUser.IsSuccess = false;
                objUser.Message = ex.Message;
                return objUser;
            }
        }
    }
}