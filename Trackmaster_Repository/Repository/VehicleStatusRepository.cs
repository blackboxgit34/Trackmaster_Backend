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
        public VehicleStatusRepository(IConfiguration configuration)
        {
            _connectionString43 = configuration.GetConnectionString("DefaultConnection43");
        }
        public async Task<List<VehicleonMapList>> GetvehicleStatusList(int userid)
        {
            var list = new List<VehicleonMapList>();
            try
            {
                using var con = new SqlConnection(_connectionString43);
                using var cmd = new SqlCommand("getVehicleStatusTM", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@custid", userid);

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
