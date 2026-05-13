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
        public async Task<List<VehicleonMapList>> GetvehicleStatusList(int userid)
        {
            var list = new List<VehicleonMapList>();
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
                    list.Add(new VehicleonMapList
                    {
                        VehName = GetString(reader["VehicleName"]),
                        VehicleStatus = GetString(reader["type"]),
                        Type = GetString(reader["statusCode"])
                    });
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
