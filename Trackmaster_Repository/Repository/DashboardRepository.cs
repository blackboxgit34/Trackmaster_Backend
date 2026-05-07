using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using Trackmaster_Model;
using Trackmaster_Repository.Interface;
using static Trackmaster_Repository.DataTypeHelper;

namespace Trackmaster_Repository.Repository
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly string _connectionString43;
        public DashboardRepository(IConfiguration configuration)
        {
            _connectionString43 = configuration.GetConnectionString("DefaultConnection43");
        }
        public async Task<VehicleStatus> GetVehicleStatus(int userid)
        {
            using var con = new SqlConnection(_connectionString43);
            using var cmd = new SqlCommand("GetVehicleStatusTrackmaster", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@custid", userid);

            await con.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            var model = new VehicleStatus();

            if (await reader.ReadAsync())
            {
                model.TotalVehicles = GetInt(reader["TotalVehicles"]);
                model.Moving = GetInt(reader["Moving"]);
                model.HighSpeed = GetInt(reader["HiSpeed"]);
                model.IgnitionON = GetInt(reader["IgnitionOn"]);
                model.Parked = GetInt(reader["Parked"]);
                model.Towed = GetInt(reader["Towed"]);
                model.Unreachable = GetInt(reader["Unreachable"]);
                model.BatteryDisconnect = GetInt(reader["BatteryDisconnect"]);
                model.Breakdown = GetInt(reader["Breakdown"]);
            }

            return model;
        }

        public async Task<VehicleUtilization> GetVehicleUtilization(int userid)
        {
            using var con = new SqlConnection(_connectionString43);
            using var cmd = new SqlCommand("GetVehicleUtilizationTrackmaster", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@custid", userid);

            await con.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            var model = new VehicleUtilization();

            if (await reader.ReadAsync())
            {
                model.TotalVehicles = GetInt(reader["Totalvehicle"]);
                model.IgnitionON = GetInt(reader["IgnitionON"]) / 3600;
                model.Moving = GetInt(reader["Moving"]) / 3600;
                model.Parked = GetInt(reader["Parked"]) / 3600;
            }

            return model;
        }

        public async Task<SpeedAnalysis> GetSpeedAnalysis(int userid)
        {
            using var con = new SqlConnection(_connectionString43);
            using var cmd = new SqlCommand("GetSpeedAnalysisTrackmaster", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@custid", userid);

            await con.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            var model = new SpeedAnalysis();

            if (await reader.ReadAsync())
            {
                model.OS = GetInt(reader["overSpeedCount"]);
                model.nonOS = GetInt(reader["nonOverSpeed"]);
            }

            return model;
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

        public async Task<List<DistanceDashModel>> GetDistanceDash(int custId, DateTime start, DateTime end)
        {
            var result = new List<DistanceDashModel>();

            try
            {
                using var con = new SqlConnection(_connectionString43);
                using var cmd = new SqlCommand("GetDistanceDash", con);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@custId", custId);
                cmd.Parameters.AddWithValue("@start", start);
                cmd.Parameters.AddWithValue("@end", end);

                await con.OpenAsync();

                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var item = new DistanceDashModel
                    {
                        BBID = reader["bbid"]?.ToString(),
                        VehicleName = reader["vehname"]?.ToString(),
                        Distance = reader["distance"] != DBNull.Value ? Convert.ToDouble(reader["distance"]): 0
                    };

                    result.Add(item);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            return result;
        }

    }
}