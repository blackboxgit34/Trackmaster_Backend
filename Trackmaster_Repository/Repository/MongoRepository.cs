using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trackmaster_Model;
using Trackmaster_Repository.Interface;
using static Trackmaster_Model.MongoModel;
using static Trackmaster_Model.Reports;
using static Trackmaster_Model.VehicleStatusModel;
using static Trackmaster_Repository.DataTypeHelper;

namespace Trackmaster_Repository.Repository
{
    public class MongoRepository : IMongoRepository
    {
        private readonly IMongoDatabase _database;
        private readonly string _connectionString43;
        private readonly string _connectionString44;

        public MongoRepository(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetConnectionString("MongoDb"));

            _database = client.GetDatabase(configuration["MongoSettings:DatabaseName"]);
            _connectionString43 = configuration.GetConnectionString("DefaultConnection43");
            _connectionString44 = configuration.GetConnectionString("DefaultConnection44");
        }
        public async Task<List<VehicleMaster>> GetLiveStatus(string pagename, DataTableRequestModel model)
        {
            var list = new List<VehicleMaster>();
            int totalCount = 0;
            try
            {
                using var con = new SqlConnection(_connectionString43);
                using var cmd = new SqlCommand("getVehicleStatusTM", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@custid", model.CustId);
                cmd.Parameters.AddWithValue("@pageName", pagename);
                // Paging Parameters
                cmd.Parameters.AddWithValue("@lbound", model.iDisplayStart);
                cmd.Parameters.AddWithValue("@ubound", model.iDisplayStart + model.iDisplayLength);
                cmd.Parameters.AddWithValue("@sortColumn", model.sortColumn);
                cmd.Parameters.AddWithValue("@sortDirection", model.sortDirection);
                cmd.Parameters.Add("@sSearch", SqlDbType.VarChar).Value = string.IsNullOrWhiteSpace(model.sSearch) || model.sSearch == "null" ? DBNull.Value : model.sSearch;
                // Output Parameter
                SqlParameter itemCountParam = new SqlParameter("@itemcount", SqlDbType.Int);
                cmd.Parameters.AddWithValue("@StatusCode", null);
                itemCountParam.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(itemCountParam);
                await con.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new VehicleMaster
                    {
                        VehName = GetString(reader["vehname"]),
                        BBID = GetString(reader["bbid"]),
                        DriverName = GetString(reader["DriverName"]),
                        MobNo = GetString(reader["Mob_No"]),
                        Type = GetString(reader["type"]),
                        model = GetString(reader["model"]),
                        overspeed = GetInt(reader["overspeed"]),

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
                        vehicle.lat = live.latitude;
                        vehicle.lng = live.longitude;
                        vehicle.speed = live.speed;
                        vehicle.lastUpdated = live.datadate;
                        vehicle.location = live.location;
                        vehicle.gsmSignal = live.gsmsignal;
                        vehicle.gpsAntConStatus = live.gsmsignal;
                        vehicle.GPSFix = live.hasfix;
                        vehicle.IgnitionStatus = live.ignitionStatus;
                        vehicle.vehBattery = live.vehicleBatteryVoltage;
                        vehicle.deviceBattery = live.vtsBatteryLevel;
                        vehicle.RemainingFuelLevel = live.fuelLevel;
                        vehicle.acSignal = live.acSignal;
                        vehicle.immobilizer = live.immobilizer;

                        // Step 4: ComputeStatus called ONLY when status filter is active
                        if (!string.IsNullOrEmpty(model.Status))
                        {
                            vehicle.Status = ComputeStatus(
                                live.speed,
                                live.ignitionStatus,
                                //live.currIgnitionStatus,
                                live.datadate,
                                vehicle.overspeed
                            );
                        }

                        //if (vehicle.RemainingFuelLevel > 0)
                        //{
                        //    int y = 0;
                        //}
                    }
                    else
                    {
                        // ✅ no mongo data — ALWAYS Unreachable, not just when filtering
                        vehicle.Status = "Unreachable";
                    }
                }

                //totalCount = itemCountParam.Value == DBNull.Value ? 0 : Convert.ToInt32(itemCountParam.Value);
                if (!string.IsNullOrEmpty(model.Status))
                {
                    list = list.Where(x => x.Status == model.Status).ToList();
                    totalCount = list.Count;
                    list = list.Skip(model.iDisplayStart).Take(model.iDisplayLength).ToList();
                }
                else
                {
                    // ✅ normal flow — SP handles paging
                    totalCount = itemCountParam.Value == DBNull.Value ? 0 : Convert.ToInt32(itemCountParam.Value);
                }


                // ✅ normal flow — SP handles paging, use SP output count
                totalCount = itemCountParam.Value == DBNull.Value ? 0 : Convert.ToInt32(itemCountParam.Value);


                if (list.Count > 0)
                {
                    list[0].TotalRecords = totalCount;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
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

        private string ComputeStatus(double speed, bool ignitionStatus, DateTime lastDate, int overspeed)
        {
            double hoursDiff = (DateTime.Now - lastDate).TotalHours;


            // when datediff(hour,m.lastdate,getdate()) > 6 then 'Unreachable'
            if (hoursDiff > 6)
                return "Unreachable";

            // when m.speed > 0 and m.speed >= overspeed and datediff <= 6 and currIgnitionStatus=0 and ignitionStatus=0 then 'High Speed'
            if (speed > 0 && speed >= 60 && hoursDiff <= 6 && ignitionStatus == false)
                return "High Speed";

            // when m.speed > 0 and m.speed < overspeed and datediff <= 6 and currIgnitionStatus=0 and ignitionStatus=0 then 'Moving'
            if (speed > 0 && speed < 60 && hoursDiff <= 6 && ignitionStatus == true)
                return "Moving";

           

            // when m.speed <= 0 and datediff <= 6 and currIgnitionStatus=0 and ignitionStatus=1 then 'Ignition On'
            if (speed <= 0 && hoursDiff <= 6 && ignitionStatus == true)
                return "Ignition On";

            // when m.speed <= 0 and datediff <= 6 and currIgnitionStatus=0 and ignitionStatus=0 then 'Ignition On'
            //if (speed <= 0 && hoursDiff <= 6 && ignitionStatus == false)
            //    return "Ignition On";


            // when m.speed <= 0 and datediff <= 6 and currIgnitionStatus=1 and ignitionStatus=1 then 'Parked'
            //if (speed <= 0 && hoursDiff <= 6 && ignitionStatus == true)
            //    return "Parked";

            // when m.speed <= 0 and datediff <= 6 and currIgnitionStatus=1 and ignitionStatus=0 then 'Parked'
            if (speed <= 0 && hoursDiff <= 6 && ignitionStatus == false)
                return "Parked";

            // when m.speed > 0 and datediff <= 6 and currIgnitionStatus=1 and ignitionStatus=1 then 'Towed'
            if (speed > 0 && hoursDiff <= 6 && ignitionStatus == true)
                return "Towed";

            // when m.speed > 0 and m.speed < overspeed and datediff <= 6 and currIgnitionStatus=0 and ignitionStatus=1 then 'Moving'
            //if (speed > 0 && speed < overspeed && hoursDiff <= 6 && ignitionStatus == true)
            //    return "Moving";

            // when m.speed > 0 and m.speed >= overspeed and datediff <= 6 and currIgnitionStatus=0 and ignitionStatus=1 then 'High Speed'
            //if (speed > 0 && speed >= overspeed && hoursDiff <= 6 && ignitionStatus == true)
            //    return "High Speed";

            return "Unknown";
        }
    }
}
