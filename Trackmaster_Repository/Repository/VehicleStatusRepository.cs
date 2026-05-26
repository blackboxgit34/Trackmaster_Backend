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
using static Trackmaster_Model.VehicleStatusModel;
using static Trackmaster_Repository.DataTypeHelper;

namespace Trackmaster_Repository.Repository
{
    public class VehicleStatusRepository : IVehicleStatusRepository
    {
        private readonly string _connectionString43;
        private readonly string _connectionString44;
        public VehicleStatusRepository(IConfiguration configuration)
        {
            _connectionString43 = configuration.GetConnectionString("DefaultConnection43");
            _connectionString44 = configuration.GetConnectionString("DefaultConnection44");
        }
        public string GetConnectionStringTableWise(string tableName)
        {
            return ((tableName.StartsWith("i", StringComparison.OrdinalIgnoreCase) || tableName.StartsWith("j", StringComparison.OrdinalIgnoreCase)) && tableName.Length > 5) ? _connectionString44 : _connectionString43;
        }

        public async Task<List<VehicleonMapList>> GetvehicleStatusList(string pagename, DataTableRequestModel model)
         {
            var list = new List<VehicleonMapList>();
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
                cmd.Parameters.AddWithValue("@StatusCode", model.Status ?? (object)DBNull.Value);
                itemCountParam.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(itemCountParam);
                await con.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new VehicleonMapList
                    {
                        VehName = GetString(reader["vehname"]),
                        VehicleStatus = GetString(reader["statusCode"]),
                        Type = GetString(reader["type"]),
                        model = GetString(reader["model"]),
                        lat = GetFloat(reader["lat"]),
                        lng = GetFloat(reader["longi"]),
                        speed = GetString(reader["speed"]),
                        location = GetString(reader["location"]),
                        lastUpdated = GetString(reader["Lastdate"]),
                        bbid = GetString(reader["bbid"]),
                        gsmSignal = GetInt(reader["gsmSignal"]),
                        gpsAntConStatus = GetInt(reader["gpsAntConStatus"]),
                        GPSFix = GetInt(reader["GPSFix"]),
                        IgnitionStatus = GetString(reader["currignitionStatus"]),
                        vehBattery = GetInt(reader["vehicleBattery"]),
                        deviceBattery = GetInt(reader["deviceBattery"]),
                        driverName = GetString(reader["DriverName"]),
                        mob_no = GetString(reader["Mob_No"]),
                    });
                }
                reader.Close();

                totalCount = itemCountParam.Value == DBNull.Value? 0: Convert.ToInt32(itemCountParam.Value);

                if (list.Count > 0)
                {
                    list[0].TotalRecords =
                        totalCount;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            return list;
        }
        public async Task<(List<PlaybackDataModel> playbackData, List<LatLongHistory> latLongData)> GetPlaybackData(string bbid, DateTime date)
        {
            var list = new List<PlaybackDataModel>();
            var listlatLong = new List<LatLongHistory>();
            try
            {
                using var con = new SqlConnection(GetConnectionStringTableWise(bbid));
                using var cmd = new SqlCommand("GetPlaybackData", con);
                cmd.CommandType = CommandType.StoredProcedure;

                DateTime selectedDate = Convert.ToDateTime(date);
                DateTime startDate = selectedDate.Date;
                DateTime endDate = selectedDate.Date.AddDays(1).AddSeconds(-1);

                cmd.Parameters.AddWithValue("@bbid", bbid);
                cmd.Parameters.AddWithValue("@startdate", startDate);
                cmd.Parameters.AddWithValue("@enddate", endDate);

                await con.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new PlaybackDataModel
                    {
                        latitude = GetDecimal(reader["latitude"]),
                        longitude = GetDecimal(reader["longitude"]),
                        location = GetString(reader["loc"]),
                        speed = GetInt(reader["speed"]),
                        datadate = GetDateTime(reader["datadate"]),
                        acignition = GetString(reader["acignition"]),
                        distance = GetDecimal(reader["distance"])
                    });

                }
                var top5 = list
                    .OrderByDescending(x => x.datadate)
                    .Take(5)
                    .ToList();
                // Add top5 lat/long records again if needed
                foreach (var item in top5.OrderBy(x => x.datadate))
                {
                    listlatLong.Add(new LatLongHistory
                    {
                        latitude = item.latitude,
                        longitude = item.longitude,
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            return (list, listlatLong);
        }
        public async Task<List<GetFuelLevelsModel>>GetFuelLevels(List<string> bbids)
        {
            List<GetFuelLevelsModel> list = new List<GetFuelLevelsModel>();
            try
            {
                using var con = new SqlConnection(_connectionString43);
                using var cmd =new SqlCommand("GetCurrentFuelLevels",con);
                cmd.CommandType = CommandType.StoredProcedure;
                DataTable dt = new DataTable();

                dt.Columns.Add("BBID");

                bbids.ForEach(x => dt.Rows.Add(x));

                SqlParameter param =
                    new SqlParameter
                    {
                        ParameterName = "@BBIDs",
                        SqlDbType = SqlDbType.Structured,
                        TypeName = "BBIDListType",
                        Value = dt
                    };

                cmd.Parameters.Add(param);
                await con.OpenAsync();
                using var reader =
                    await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new GetFuelLevelsModel
                    {
                        BBID =GetString(reader["BBID"]),

                        CurrentFuelLevel =
                            GetDecimal(
                                reader["CurrentFuelLevel"]),

                        RemainingFuelLevel =
                            GetDecimal(
                                reader["RemainingFuelLevel"]),

                        TotalFuel =
                            GetDecimal(
                                reader["TotalFuel"]),

                        LastDateTime =
                            GetString(reader["LastDateTime"]),

                        DisconnectedData =
                            GetString(reader["DisconnectedData"])
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return list;
        }
    }
}
