using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using Trackmaster_Model;
using Trackmaster_Repository.Interface;
using static Trackmaster_Model.Reports;
using static Trackmaster_Repository.DataTypeHelper;

namespace Trackmaster_Repository.Repository
{
    public class GeofenceRepository : IGeofenceRepository
    {
        private readonly string _connectionString43;
        public GeofenceRepository(IConfiguration configuration)
        {
            _connectionString43 = configuration.GetConnectionString("DefaultConnection43");
        }
        public async Task<string> SaveGeofence(GeofenceModel model)
        {
            try
            {
                using var con = new SqlConnection(_connectionString43);
                await con.OpenAsync();

                using var tran = con.BeginTransaction();

                try
                {
                    if (model.FenceId == 0)
                    {
                        using var cmd = new SqlCommand("SetGeofence", con, tran);

                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@FenceName", model.FenceName);
                        cmd.Parameters.AddWithValue("@FenceType", model.FenceType);

                        model.FenceId = Convert.ToInt32(
                            await cmd.ExecuteScalarAsync()
                        );

                        if (model.FenceType == "Polygon")
                        {
                            foreach (var vehicle in model.vehicleLists)
                            {
                                using var mapCmd = new SqlCommand(
                                    @"INSERT INTO FenceDeviceMaping(FenceIDFK,BBID)
                              VALUES(@FenceIDFK,@BBID)",
                                    con,
                                    tran);

                                mapCmd.Parameters.AddWithValue("@FenceIDFK", model.FenceId);
                                mapCmd.Parameters.AddWithValue("@BBID", vehicle.BBID);

                                await mapCmd.ExecuteNonQueryAsync();
                            }
                        }
                        else
                        {
                            foreach (var vehicle in model.vehicleLists)
                            {
                                using var mapCmd = new SqlCommand(
                                    @"INSERT INTO FenceDeviceMapingCircle(FenceIDFK,BBID)
                              VALUES(@FenceIDFK,@BBID)",
                                    con,
                                    tran);

                                mapCmd.Parameters.AddWithValue("@FenceIDFK", model.FenceId);
                                mapCmd.Parameters.AddWithValue("@BBID", vehicle.BBID);

                                await mapCmd.ExecuteNonQueryAsync();
                            }
                        }
                    }
                    else
                    {
                        if (model.FenceType == "Polygon")
                        {
                            using var deleteCmd = new SqlCommand(
                                "DELETE FROM FencePoints WHERE FenceIDFK=@FenceIDFK",
                                con,
                                tran);

                            deleteCmd.Parameters.AddWithValue("@FenceIDFK", model.FenceId);

                            await deleteCmd.ExecuteNonQueryAsync();
                        }
                        else
                        {
                            using var deleteCmd = new SqlCommand(
                                "DELETE FROM FencePointsCircle WHERE FenceIDFK=@FenceIDFK",
                                con,
                                tran);

                            deleteCmd.Parameters.AddWithValue("@FenceIDFK", model.FenceId);

                            await deleteCmd.ExecuteNonQueryAsync();
                        }
                    }

                    if (model.FenceId <= 0)
                        throw new Exception("Invalid FenceId");

                    if (model.FenceType == "Polygon")
                    {
                        foreach (var point in model.latLongList)
                        {
                            using var pointCmd = new SqlCommand(
                                @"INSERT INTO FencePoints
                          (FenceIDFK,Lat,[Long])
                          VALUES
                          (@FenceIDFK,@Lat,@Long)",
                                con,
                                tran);

                            pointCmd.Parameters.AddWithValue("@FenceIDFK", model.FenceId);
                            pointCmd.Parameters.AddWithValue("@Lat", point.latitude);
                            pointCmd.Parameters.AddWithValue("@Long", point.longitude);

                            await pointCmd.ExecuteNonQueryAsync();
                        }
                    }
                    else
                    {
                        foreach (var point in model.latLongList)
                        {
                            using var pointCmd = new SqlCommand(
                                @"INSERT INTO FencePointsCircle
                          (FenceIDFK,Lat,[Long],Radius)
                          VALUES
                          (@FenceIDFK,@Lat,@Long,@Radius)",
                                con,
                                tran);

                            pointCmd.Parameters.AddWithValue("@FenceIDFK", model.FenceId);
                            pointCmd.Parameters.AddWithValue("@Lat", point.latitude);
                            pointCmd.Parameters.AddWithValue("@Long", point.longitude);
                            pointCmd.Parameters.AddWithValue("@Radius", model.Radius);

                            await pointCmd.ExecuteNonQueryAsync();
                        }
                    }

                    await tran.CommitAsync();

                    return "Success";
                }
                catch
                {
                    await tran.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public async Task<(List<GeofenceModel> geofenceList, int TotalCount)> GetGeofenceList(DataTableRequestModel model)
        {
            var result = new List<GeofenceModel>();
            int TotalCount = 0;
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString43))
                using (SqlCommand cmd = new SqlCommand("GetGeofenceList", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@custId", model.CustId);
                    cmd.Parameters.AddWithValue("@iDisplayStart", model.iDisplayStart);
                    cmd.Parameters.AddWithValue("@iDisplayLength", model.iDisplayLength);
                    cmd.Parameters.AddWithValue("@sortColumn", model.sortColumn);
                    cmd.Parameters.AddWithValue("@sortDirection", model.sortDirection);
                    cmd.Parameters.AddWithValue("@sSearch", model.sSearch);

                    SqlParameter totalCountParam =
                        new SqlParameter("@TotalCount", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };

                    cmd.Parameters.Add(totalCountParam);

                    await con.OpenAsync();

                    using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            result.Add(new GeofenceModel
                            {
                                FenceId = GetInt(dr["FenceId"]),
                                FenceName = GetString(dr["FenceName"]),
                                FenceType = GetString(dr["FenceType"]),
                                IsActive = GetBool(dr["IsActive"]),
                                vehicleLists = new List<VehicleList>()
                            });
                        }
                    }

                    TotalCount = Convert.ToInt32(totalCountParam.Value);
                    con.CloseAsync();
                }
                var tasks = result.Select(async item =>
                {
                    using (SqlConnection deviceCon = new SqlConnection(_connectionString43))
                    using (SqlCommand deviceCmd = new SqlCommand("GetGeofenceDevicesList", deviceCon))
                    {
                        deviceCmd.CommandType = CommandType.StoredProcedure;
                        deviceCmd.Parameters.AddWithValue("@FenceId", item.FenceId);
                        deviceCmd.Parameters.AddWithValue("@FenceType", item.FenceType);
                        deviceCmd.Parameters.AddWithValue("@CustId", model.CustId);
                        await deviceCon.OpenAsync();

                        using (SqlDataReader dr = await deviceCmd.ExecuteReaderAsync())
                        {
                            while (await dr.ReadAsync())
                            {
                                item.vehicleLists.Add(new VehicleList
                                {
                                    VehName = GetString(dr["VehName"]),
                                    BBID = GetString(dr["BBID"]),
                                    Type = GetString(dr["Type"])
                                });
                            }
                        }

                        deviceCon.CloseAsync();
                    }
                });

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return (result, TotalCount);
        }
    }
}
