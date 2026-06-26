using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Data;
using Trackmaster_Model;
using Trackmaster_Repository.Interface;
using static Trackmaster_Model.MongoModel;
using static Trackmaster_Repository.DataTypeHelper;

namespace Trackmaster_Repository.Repository
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly string _connectionString43;
        private readonly IMongoDatabase _database;
        private readonly string _connectionString44;
        public DashboardRepository(IConfiguration configuration)
        {
            _connectionString43 = configuration.GetConnectionString("DefaultConnection43");
            var client = new MongoClient(configuration.GetConnectionString("MongoDb"));
            _database = client.GetDatabase(configuration["MongoSettings:DatabaseName"]);
            _connectionString44 = configuration.GetConnectionString("DefaultConnection44");
        }

        //public async Task<VehicleStatus> GetVehicleStatus(int userid)
        //{
        //    var model = new VehicleStatus();
        //    try
        //    {
        //        using var con = new SqlConnection(_connectionString43);
        //        using var cmd = new SqlCommand("GetVehicleStatusTrackmaster", con);
        //        cmd.CommandType = CommandType.StoredProcedure;
        //        cmd.Parameters.AddWithValue("@custid", userid);

        //        await con.OpenAsync();
        //        using var reader = await cmd.ExecuteReaderAsync();
        //        if (await reader.ReadAsync())
        //        {
        //            model.TotalVehicles = GetInt(reader["TotalVehicles"]);
        //            model.Moving = GetInt(reader["Moving"]);
        //            model.HighSpeed = GetInt(reader["HiSpeed"]);
        //            model.IgnitionON = GetInt(reader["IgnitionOn"]);
        //            model.Parked = GetInt(reader["Parked"]);
        //            model.Towed = GetInt(reader["Towed"]);
        //            model.Unreachable = GetInt(reader["Unreachable"]);
        //            model.BatteryDisconnect = GetInt(reader["BatteryDisconnect"]);
        //            model.Breakdown = GetInt(reader["Breakdown"]);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    return model;
        //}

        public async Task<List<VehicleMaster>> GetVehicleStatus(int userid)
        {           
            var list = new List<VehicleMaster>();
            try
            {
                using var con = new SqlConnection(_connectionString43);
                using var cmd = new SqlCommand("GetVehicleStatusTrackmaster_new", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@custid", userid);

                await con.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new VehicleMaster
                    {
                        BBID = GetString(reader["BBID"]),
                    });
                }
                reader.Close();
                var bbids = list.Select(x => x.BBID).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                // Mongo Data
                var liveData = await GetLiveDataByBbids(bbids);

                // Create Dictionary
                var liveLookup = liveData.ToDictionary(x => x.bbid);
                foreach (var vehicle in list)
                {
                    var live = liveData.FirstOrDefault(x => x.bbid == vehicle.BBID);

                    if (live != null)
                    {
                        // Map Mongo fields to VehicleMaster
                        vehicle.speed = live.speed;
                        vehicle.lastUpdated = live.datadate;
                        vehicle.IgnitionStatus = live.ignitionStatus;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return list;
        }

        public async Task<List<DeviceLiveData>> GetLiveDataByBbids(List<string> bbids)
        {
            var collection = _database.GetCollection<DeviceLiveData>("bbmain");
            // loop here

            var filter = Builders<DeviceLiveData>.Filter.In(x => x.bbid, bbids);

            var result = await collection.Find(filter).ToListAsync();

            return result;
        }

        public async Task<VehicleUtilization> GetVehicleUtilization(int userid)
        {
            var model = new VehicleUtilization();
            try
            {
                using var con = new SqlConnection(_connectionString43);
                using var cmd = new SqlCommand("GetVehicleUtilizationTrackmaster", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@custid", userid);

                await con.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    model.TotalVehicles = GetInt(reader["Totalvehicle"]);
                    model.IgnitionON = GetInt(reader["IgnitionON"]) / 3600;
                    model.Moving = GetInt(reader["Moving"]) / 3600;
                    model.Parked = GetInt(reader["Parked"]) / 3600;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            return model;
        }

        public async Task<SpeedAnalysis> GetSpeedAnalysis(int userid, DateTime start, DateTime end)
        {
            var model = new SpeedAnalysis();
            try
            {


                if (start == DateTime.MinValue)
                {
                    start = GetDateTime(DateTime.Today.AddDays(-1));
                }
                if (end == DateTime.MinValue)
                {
                    end = GetDateTime(DateTime.Today.AddSeconds(-1));
                }
                using var con = new SqlConnection(_connectionString43);
                using var cmd = new SqlCommand("GetSpeedAnalysisTrackmaster", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@custid", userid);
                cmd.Parameters.AddWithValue("@stdate", start);
                cmd.Parameters.AddWithValue("@edDate", end);
                await con.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    model.OS = GetInt(reader["overSpeedCount"]);
                    model.nonOS = GetInt(reader["nonOverSpeed"]);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            return model;
        }

        public async Task<List<IdlingDuration>> GetIdlingDuration(int userid)
        {
            var list = new List<IdlingDuration>();

            try
            {
                using var con = new SqlConnection(_connectionString43);
                using var cmd = new SqlCommand("GetIdlingDuration", con);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@custid", userid);

                await con.OpenAsync();

                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    list.Add(new IdlingDuration
                    {
                        VehicleName = GetString(reader["VehicleName"]),
                        TotalIdlingHours = GetString(reader["TotalIdlingHours"])
                    });
                }
            }
            catch (Exception ex)
            {
                // You can log exception here
                throw new Exception("Error while fetching idling duration data.", ex);
            }

            return list;
        }

        public async Task<List<VehicleList>> GetAllVehicleListByCustId(int userid)
        {
            var list = new List<VehicleList>();
            try
            {
                using var con = new SqlConnection(_connectionString43);
                using var cmd = new SqlCommand("GetVehicleData", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@custid", userid);

                await con.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new VehicleList
                    {
                        VehName = GetString(reader["VehicleName"]),
                        BBID = GetString(reader["BBID"]),
                        Type = GetString(reader["Type"])
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            return list;
        }
        public async Task<List<VehicleType>> GetAllVehicleTypes()
        {
            var list = new List<VehicleType>();
            try
            {
                using var con = new SqlConnection(_connectionString43);
                using var cmd = new SqlCommand("GetAllVehicleTypes", con);
                cmd.CommandType = CommandType.StoredProcedure;

                await con.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new VehicleType
                    {
                        Id = GetInt(reader["Id"]),
                        TypeName = GetString(reader["TypeName"])
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            return list;
        }
        public async Task<List<OverSpeedReport>> GetOverSpeedGraphData(int custid, string bbid)
        {
            var list = new List<OverSpeedReport>();
            try
            {
                using var con = new SqlConnection(_connectionString43);
                using var cmd = new SqlCommand("OverSpeedGraphData", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@custid", custid);
                cmd.Parameters.AddWithValue("@bbid", bbid);

                await con.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new OverSpeedReport
                    {
                        DateTime = GetDateTime(reader["ReportDay"]).ToString("yyyy-MM-dd"),
                        overspeedCount = GetInt(reader["overSpeedCount"]),
                        OverCustomCount = GetInt(reader["nonOverSpeed"])
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            return list;
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
                        BBID = GetString(reader["bbid"]),
                        VehicleName = GetString(reader["vehname"]),
                        Distance = GetDouble(reader["distance"])
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

        public async Task<List<AverageDrivingHours>> GetAverageDrivingHours(int custId)
        {
            var list = new List<AverageDrivingHours>();
            try
            {
                using var con = new SqlConnection(_connectionString43);
                using var cmd = new SqlCommand("AverageDrivingHours", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@custId", custId);
                await con.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {

                    list.Add(new AverageDrivingHours
                    {
                        ReportDate = GetDateTime(reader["ReportDate"]).ToString("yyyy-MM-dd"),
                        vehname = GetString(reader["vehname"]),
                        BBID = GetString(reader["BBID"]),
                        driving_time = GetInt(reader["driving_time"])
                    });

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            return list;


        }
    }
}