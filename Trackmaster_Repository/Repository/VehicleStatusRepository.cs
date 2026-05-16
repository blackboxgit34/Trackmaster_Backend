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

                cmd.Parameters.AddWithValue("@ubound",model.iDisplayStart + model.iDisplayLength);

                // Search Parameter
                //cmd.Parameters.AddWithValue("@sSearch",model.sSearch);
                cmd.Parameters.Add("@sSearch", SqlDbType.VarChar).Value =
                   string.IsNullOrWhiteSpace(model.sSearch) || model.sSearch == "null"
                       ? DBNull.Value
                       : model.sSearch;
                // Output Parameter
                SqlParameter itemCountParam = new SqlParameter("@itemcount",SqlDbType.Int);

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
                        IgnitionStatus = GetString(reader["currignitionStatus"]),
                    });
                }
                reader.Close();

                totalCount = Convert.ToInt32(
                    itemCountParam.Value
                );

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
        public async Task<List<PlaybackDataModel>> GetPlaybackData(string bbid, DateTime date)
        {
            var list = new List<PlaybackDataModel>();
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
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            return list;
        }
    }
}
