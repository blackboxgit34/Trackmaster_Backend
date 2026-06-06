using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using Trackmaster_Model;
using Trackmaster_Repository.Interface;
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

        public async Task<bool> LocationExist(double lat, double longi, int custid)
        {
            try
            {
                using var con = new SqlConnection(_connectionString43);
                await con.OpenAsync();
                using var tran = con.BeginTransaction();
                try
                {
                    using var cmd = new SqlCommand("CustLocationExists", con, tran);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@lat", lat);
                    cmd.Parameters.AddWithValue("@longi", longi);
                    cmd.Parameters.AddWithValue("@custid", custid);
                    var loc = await cmd.ExecuteScalarAsync();
                    //var loc = Convert.ToString(await cmd.ExecuteScalarAsync());
                    using var poiCmd = new SqlCommand("GetPOICount", con, tran);
                    poiCmd.CommandType = CommandType.StoredProcedure;
                    poiCmd.Parameters.AddWithValue("@custid", custid);
                    var poiCount = GetInt(await poiCmd.ExecuteScalarAsync());
                    await tran.CommitAsync();

                    //if (string.IsNullOrEmpty(LOC))
                    //{
                    //    return "true";
                    //}
                    //else
                    //{
                    //    return "FALSE";
                    //}

                    // No record found
                    return loc == null || loc == DBNull.Value;
                }
                catch
                {
                    await tran.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<Boolean> SavePOI(double lat, double longi, int custid, string location, string radius)
        {
            try
            {
                using var con = new SqlConnection(_connectionString43);
                await con.OpenAsync();
                using var tran = con.BeginTransaction();
                try
                {
                    using var cmd = new SqlCommand("SavePOI", con, tran);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@lat", lat);
                    cmd.Parameters.AddWithValue("@longi", longi);
                    cmd.Parameters.AddWithValue("@custid", custid);
                    cmd.Parameters.AddWithValue("@location", location);
                    cmd.Parameters.AddWithValue("@radius", radius);
                    var affectedRows = GetInt(await cmd.ExecuteScalarAsync());
                    await tran.CommitAsync();
                    if (affectedRows > 0)
                    {
                        return true;

                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception)
                {
                    await tran.RollbackAsync();

                    throw;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public async Task<List<PoiList>> GetPOI(string custId)
        {
            var poiList = new List<PoiList>();
            try
            {
                using var con = new SqlConnection(_connectionString43);
                await con.OpenAsync();
                using var cmd = new SqlCommand("GetPOI", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CustId", custId);

                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    poiList.Add(new PoiList
                    {
                      
                        id = GetString(reader["id"]),
                        lat = GetString(reader["lat"]),
                        lng = GetString(reader["longi"]),
                        details = GetString(reader["details"]),
                        StandardDistance = GetString(reader["StandardDistance"]),
                        poitype = GetString(reader["poitype"])
                    });
                }

                return poiList;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting POI data: {ex.Message}", ex);
            }
        }
    }
}
