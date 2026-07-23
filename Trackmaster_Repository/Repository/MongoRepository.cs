using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
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
                       

                    });
                }
                reader.Close();
                await con.CloseAsync();
                var bbids = list.Select(x => x.BBID).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                // Mongo Data
                var liveData = await GetLiveDataByBbids(bbids);

                // Create Dictionary
                var liveLookup = liveData.ToDictionary(x => x.bbid);
                DateTime beginDate = DateTime.Today;     
                DateTime endDate = DateTime.Now;
                foreach (var vehicle in list)
                {
                    var live = liveData.FirstOrDefault(x => x.bbid == vehicle.BBID);

                    if (live != null)
                    {
                        // Map Mongo fields to VehicleMaster
                        vehicle.lat = live.latitude;
                        vehicle.lng = live.longitude;
                        vehicle.speed = live.speed;
                        vehicle.lastUpdated = live.dataDate;
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
                        vehicle.distance = live.distance;
                        vehicle.distance0h = live.distance0h;
                        var wHour = live.ioJson?.wHour ?? 0;
                        var wHour0h = live.ioJson?.wHour0h ?? 0;
                        vehicle.wHour = wHour;
                        vehicle.wHour0h = wHour0h;
                        vehicle.BBID= live.bbid;
                        vehicle.VehName= live.vehName;
                        vehicle.overSpeedLimit = live.overSpeedLimit;

                        // ✅ Today's distance = total distance - distance at 00:00
                        vehicle.todayDistance = live.distance >= live.distance0h
                            ? Math.Round(live.distance - live.distance0h, 2)
                            : 0;

                        // ✅ Today's running hours = total wHour - wHour at 00:00
                        vehicle.todayWHour = wHour >= wHour0h
                             ? Math.Round(wHour - wHour0h, 2)
                                        : 0;

                        // Step 4: ComputeStatus called ONLY when status filter is active
                        if (!string.IsNullOrEmpty(model.Status))
                        {
                            vehicle.Status = ComputeStatus(
                                live.speed,
                                live.ignitionStatus,
                                //live.currIgnitionStatus,
                                live.dataDate,
                                vehicle.overSpeedLimit
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
                    // ✅ Battery disconnection alert count per bbid (bbid used as table name)
                    if (!string.IsNullOrEmpty(vehicle.BBID))
                    {
                        try
                        {
                            vehicle.alertsCount = BatteryDisconnectionCount(beginDate, endDate, vehicle.BBID);
                        }
                        catch
                        {
                            // table may not exist for this bbid, etc. — keep count 0
                            vehicle.alertsCount = 0;
                        }
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
            var collection = _database.GetCollection<DeviceLiveData>("LiveStatus");
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

        public int BatteryDisconnectionCount(DateTime beginDate, DateTime endDate, string tblName)
        {
            string p1 = beginDate.ToString("yyyy.MM.dd HH:mm:ss");
            string p2 = endDate.ToString("yyyy.MM.dd HH:mm:ss");
            try
            {
                int count = 0;
                bool flag = false;

                using var con = new SqlConnection(_connectionString43);
                using var cmd = new SqlCommand(
                    "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED; " +
                    "select vehbatvoltage, datadate from " + tblName +
                    " where datadate between '" + p1 + "' and '" + p2 + "' order by datadate asc", con);
                cmd.CommandType = CommandType.Text;

                con.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int volt = Convert.ToInt32(reader["vehbatvoltage"]);

                    if (volt < 5 && flag == false)
                    {
                        // start of a disconnection   
                        flag = true;
                    }
                    else if (volt >= 5 && flag == true)
                    {
                        // disconnection ended -> one complete event
                        count++;
                        flag = false;
                    }
                }
                reader.Close();

                // still disconnected at end of range -> count the open event
                if (flag == true)
                {
                    count++;
                }

                return count;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
