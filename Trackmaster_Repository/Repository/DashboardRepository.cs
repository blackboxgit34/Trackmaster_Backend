using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
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
        //public DashboardData GetDashboardData(int userid)
        //{
        //    var dashboardDataModel = new DashboardData();
        //    try
        //    {
        //        using (SqlConnection con = new SqlConnection(_connectionString43))
        //        {
        //            con.Open();
        //            using (SqlCommand cmd = new SqlCommand("GetDashboardDataTrackmaster", con))
        //            {
        //                cmd.CommandType = CommandType.StoredProcedure;
        //                cmd.Parameters.AddWithValue("@custid", userid);


        //                using (SqlDataReader reader = cmd.ExecuteReader())
        //                {
        //                    if (reader.Read())
        //                    {
        //                        var VehicleStatusModel = new VehicleStatus();
        //                        VehicleStatusModel.TotalVehicles = GetInt(reader["TotalVehicles"]);
        //                        VehicleStatusModel.Moving = GetInt(reader["Moving"]);
        //                        VehicleStatusModel.HighSpeed = GetInt(reader["HiSpeed"]);
        //                        VehicleStatusModel.IgnitionON = GetInt(reader["IgnitionOn"]);
        //                        VehicleStatusModel.Parked = GetInt(reader["Parked"]);
        //                        VehicleStatusModel.Towed = GetInt(reader["Towed"]);
        //                        VehicleStatusModel.Unreachable = GetInt(reader["Unreachable"]);
        //                        VehicleStatusModel.BatteryDisconnect = GetInt(reader["BatteryDisconnect"]);
        //                        VehicleStatusModel.Breakdown = GetInt(reader["Breakdown"]);
        //                        dashboardDataModel.vehicleStatus = VehicleStatusModel;
        //                    }
        //                    if (reader.NextResult() && reader.Read())
        //                    {
        //                        var VehicleUtilizationModel = new VehicleUtilization();
        //                        VehicleUtilizationModel.IgnitionON = GetInt(reader["IgnitionON"]);
        //                        VehicleUtilizationModel.Moving = GetInt(reader["Moving"]);
        //                        VehicleUtilizationModel.Parked = GetInt(reader["Parked"]);
        //                        VehicleUtilizationModel.TotalVehicles = GetInt(reader["Totalvehicle"]);
        //                        VehicleUtilizationModel.IgnitionON = VehicleUtilizationModel.IgnitionON / 3600;
        //                        VehicleUtilizationModel.Moving = VehicleUtilizationModel.Moving / 3600;
        //                        VehicleUtilizationModel.Parked = VehicleUtilizationModel.Parked / 3600;
        //                        dashboardDataModel.vehicleUtilization = VehicleUtilizationModel;
        //                    }
        //                    if (reader.NextResult() && reader.Read())
        //                    {
        //                        var SpeedAnalysisModel = new SpeedAnalysis();
        //                        SpeedAnalysisModel.OS = GetInt(reader["overSpeedCount"]);
        //                        SpeedAnalysisModel.nonOS = GetInt(reader["nonOverSpeed"]);
        //                        dashboardDataModel.speedAnalysis = SpeedAnalysisModel;
        //                    }
        //                }
        //            }
        //            con.Close();
        //        }
        //        dashboardDataModel.IsSuccess = true;
        //        dashboardDataModel.Message = "Dashboard data retrieved successfully";
        //        return dashboardDataModel;
        //    }
        //    catch (Exception ex)
        //    {
        //        dashboardDataModel.IsSuccess = false;
        //        dashboardDataModel.Message = ex.Message;
        //        return dashboardDataModel;
        //    }
        //}
    }
}