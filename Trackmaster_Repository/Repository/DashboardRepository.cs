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
    public class DashboardRepository :IDashboardRepository
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

    }
}
