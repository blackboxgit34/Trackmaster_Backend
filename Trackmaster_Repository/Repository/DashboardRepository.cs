using HMSCL.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
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
        public DashboardData GetDashboardData(int userid)
        {
            var objUser = new DashboardData();
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString43))
                {
                    con.Open();

                    using (SqlCommand cmd = new SqlCommand("getDashTotalvehicle", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@custid", userid);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                objUser.TotalVehicles = Convert.ToInt32(reader["VehicleCount"]);
                            }
                        }
                    }
                    objUser.IsSuccess = true;
                    objUser.Message = "Vehicle info fetched successfully";
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                return new DashboardData
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }

            return new DashboardData
            {
                IsSuccess = true,
                Message = "Dashboard data retrieved successfully",
                TotalVehicles = objUser.TotalVehicles
            };
        }



        public List<VehicleList> GetAllVehicleListByCustId(int custId)
        {
            var list = new List<VehicleList>();

            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString43))
                {
                    con.Open();

                    using (SqlCommand cmd = new SqlCommand("GetVehicleData", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@custid", custId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list.Add(new VehicleList
                                {
                                    VehName = reader["VehicleName"]?.ToString(),
                                    BBID = reader["BBID"]?.ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<VehicleList>();
            }

            return list;
        }


      
        public OverSpeedReport GetOverSpeedGraphReport(int custid,string bbid)
        {
            OverSpeedReport overSpeedReport = new OverSpeedReport();
            overSpeedReport.vehicleList = new List<OverSpeedAnalysisEx>();

            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString43))
                {
                    using (SqlCommand cmd = new SqlCommand("GrpahOverSpeedNew", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@custid", custid);
                        cmd.Parameters.AddWithValue("@bbid", bbid);

                        con.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read()) // ✅ multiple rows (7 days)
                            {
                                overSpeedReport.vehicleList.Add(new OverSpeedAnalysisEx
                                {
                                    // use existing fields (no new model)
                                    DateTime = reader["ReportDay"] == DBNull.Value
                                        ? ""
                                        : Convert.ToDateTime(reader["ReportDay"]).ToString("yyyy-MM-dd"),

                                    overspeedCount = reader["overSpeedCount"] == DBNull.Value
                                        ? 0
                                        : Convert.ToInt32(reader["overSpeedCount"]),

                                    OverCustomCount = reader["nonOverSpeed"] == DBNull.Value
                                        ? 0
                                        : Convert.ToInt32(reader["nonOverSpeed"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            return overSpeedReport;
        }

    }
}
