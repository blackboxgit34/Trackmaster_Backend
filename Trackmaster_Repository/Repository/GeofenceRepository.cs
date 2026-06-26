using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
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


                            using var deleteDeviceCmd = new SqlCommand(
                                "DELETE FROM FenceDeviceMaping WHERE FenceIDFK=@FenceIDFK",
                                con,
                                tran);

                            deleteDeviceCmd.Parameters.AddWithValue("@FenceIDFK", model.FenceId);

                            await deleteDeviceCmd.ExecuteNonQueryAsync();
                        }
                        else
                        {
                            using var deleteCmd = new SqlCommand(
                                "DELETE FROM FencePointsCircle WHERE FenceIDFK=@FenceIDFK",
                                con,
                                tran);

                            deleteCmd.Parameters.AddWithValue("@FenceIDFK", model.FenceId);

                            await deleteCmd.ExecuteNonQueryAsync();


                            using var deleteDeviceCmd = new SqlCommand(
                               "DELETE FROM FenceDeviceMapingCircle WHERE FenceIDFK=@FenceIDFK",
                               con,
                               tran);

                            deleteDeviceCmd.Parameters.AddWithValue("@FenceIDFK", model.FenceId);

                            await deleteDeviceCmd.ExecuteNonQueryAsync();
                        }
                    }

                    if (model.FenceId <= 0)
                        throw new Exception("Invalid FenceId");

                    if (model.FenceType == "Polygon")
                    {
                        foreach (var point in model.latLongList)
                        {
                            using var pointCmd = new SqlCommand(
                                @"INSERT INTO FencePoints (FenceIDFK,Lat,[Long]) VALUES (@FenceIDFK,@Lat,@Long)",
                                con,
                                tran);

                            pointCmd.Parameters.AddWithValue("@FenceIDFK", model.FenceId);
                            pointCmd.Parameters.AddWithValue("@Lat", point.latitude);
                            pointCmd.Parameters.AddWithValue("@Long", point.longitude);

                            await pointCmd.ExecuteNonQueryAsync();
                        }

                        foreach (var vehicle in model.vehicleLists)
                        {
                            using var mapCmd = new SqlCommand(
                                @"INSERT INTO FenceDeviceMaping(FenceIDFK,BBID) VALUES(@FenceIDFK,@BBID)",
                                con,
                                tran);

                            mapCmd.Parameters.AddWithValue("@FenceIDFK", model.FenceId);
                            mapCmd.Parameters.AddWithValue("@BBID", vehicle.BBID);

                            await mapCmd.ExecuteNonQueryAsync();
                        }
                    }
                    else
                    {
                        foreach (var point in model.latLongList)
                        {
                            using var pointCmd = new SqlCommand(
                                @"INSERT INTO FencePointsCircle(FenceIDFK,Lat,[Long],Radius) VALUES (@FenceIDFK,@Lat,@Long,@Radius)",
                                con,
                                tran);

                            pointCmd.Parameters.AddWithValue("@FenceIDFK", model.FenceId);
                            pointCmd.Parameters.AddWithValue("@Lat", point.latitude);
                            pointCmd.Parameters.AddWithValue("@Long", point.longitude);
                            pointCmd.Parameters.AddWithValue("@Radius", model.Radius);

                            await pointCmd.ExecuteNonQueryAsync();
                        }

                        foreach (var vehicle in model.vehicleLists)
                        {
                            using var mapCmd = new SqlCommand(
                                @"INSERT INTO FenceDeviceMapingCircle(FenceIDFK,BBID) VALUES(@FenceIDFK,@BBID)",
                                con,
                                tran);

                            mapCmd.Parameters.AddWithValue("@FenceIDFK", model.FenceId);
                            mapCmd.Parameters.AddWithValue("@BBID", vehicle.BBID);

                            await mapCmd.ExecuteNonQueryAsync();
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
                    using var cmd = new SqlCommand("New_Track_CustLocationExists", con, tran);
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
                    using var cmd = new SqlCommand("New_Track_SavePOI", con, tran);
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
                using var cmd = new SqlCommand("New_Tarck_GetPOI", con);
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

        public async Task<ManagePoiResponse> ManagePoi(DataTableRequestModel request, string? id)
        {
            var response = new ManagePoiResponse();

            try
            {
                using var con = new SqlConnection(_connectionString43);
                await con.OpenAsync();

                using var cmd = new SqlCommand("GetPoi_TM", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue(
                    "@LowerBound",
                    request.iDisplayStart);

                cmd.Parameters.AddWithValue(
                    "@UpperBound",
                    request.iDisplayStart + request.iDisplayLength);

                cmd.Parameters.AddWithValue(
                    "@custId",
                    request.CustId);

                cmd.Parameters.AddWithValue(
                    "@searchText",
                    string.IsNullOrWhiteSpace(request.sSearch)
                        ? DBNull.Value
                        : request.sSearch);

                cmd.Parameters.AddWithValue(
                    "@id",
                    string.IsNullOrWhiteSpace(id)
                        ? DBNull.Value
                        : id);

                var itemCountParam = new SqlParameter("@ItemCount", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };

                cmd.Parameters.Add(itemCountParam);

                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    response.Data.Add(new ManagePoi
                    {
                        id = GetString(reader["id"]),
                        custid = GetString(reader["custid"]),
                        PoiName = GetString(reader["PoiName"]),
                        Mobileno = GetString(reader["Mobileno"]),
                        Latitude = GetString(reader["Latitude"]),
                        Longitude = GetString(reader["Longitude"]),
                        Radius = GetString(reader["Radius"]),
                        POIStatus = GetString(reader["POIStatus"]),
                        Approve = GetString(reader["Approve"])
                    });
                }

                await reader.CloseAsync();

                response.ItemCount =
                    itemCountParam.Value != DBNull.Value
                        ? Convert.ToInt32(itemCountParam.Value)
                        : 0;

                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Error getting POI data: {ex.Message}", ex);
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
                    cmd.Parameters.AddWithValue("@sortColumn", model.sortColumn ?? "");
                    cmd.Parameters.AddWithValue("@sortDirection", model.sortDirection ?? "");
                    cmd.Parameters.AddWithValue("@sSearch", model.sSearch ?? "");

                    SqlParameter totalCountParam = new SqlParameter("@TotalCount", SqlDbType.Int)
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
                                Radius = GetString(dr["Radius"]),
                                vehicleLists = new List<VehicleList>(),
                                latLongList = new List<LatLongHistory>()
                            });
                        }
                    }

                    TotalCount = totalCountParam.Value != DBNull.Value
                        ? Convert.ToInt32(totalCountParam.Value)
                        : 0;
                }

                var latLongTasks = result.Select(async item =>
                {
                    try
                    {
                        using (SqlConnection con = new SqlConnection(_connectionString43))
                        using (SqlCommand cmd = new SqlCommand("GetGeofenceLatLongList", con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.AddWithValue("@FenceId", item.FenceId);
                            cmd.Parameters.AddWithValue("@FenceType", item.FenceType ?? "");

                            await con.OpenAsync();

                            using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                            {
                                while (await dr.ReadAsync())
                                {
                                    item.latLongList.Add(new LatLongHistory
                                    {
                                        latitude = GetDecimal(dr["Lat"]),
                                        longitude = GetDecimal(dr["Long"])
                                    });
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"LatLong Error FenceId={item.FenceId}: {ex.Message}");
                    }
                });

                await Task.WhenAll(latLongTasks);

                var vehicleTasks = result.Select(async item =>
                {
                    try
                    {
                        using (SqlConnection con = new SqlConnection(_connectionString43))
                        using (SqlCommand cmd = new SqlCommand("GetGeofenceDevicesList", con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.AddWithValue("@FenceId", item.FenceId);
                            cmd.Parameters.AddWithValue("@FenceType", item.FenceType ?? "");
                            cmd.Parameters.AddWithValue("@CustId", model.CustId);

                            await con.OpenAsync();

                            using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
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
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Vehicle Error FenceId={item.FenceId}: {ex.Message}");
                    }
                });

                await Task.WhenAll(vehicleTasks);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetGeofenceList Error: {ex}");
            }

            return (result, TotalCount);
        }
        public async Task<bool> DeleteGeofence(int FenceId, string Type)
        {
            SqlTransaction tran = null;

            try
            {
                using var con = new SqlConnection(_connectionString43);
                await con.OpenAsync();

                tran = con.BeginTransaction();

                // Delete Mapping
                using (var cmd = new SqlCommand(
                    $"DELETE FROM {(Type == "Circle" ? "FenceDeviceMapingCircle" : "FenceDeviceMaping")} WHERE FenceIDFK = @FenceId",
                    con, tran))
                {
                    cmd.Parameters.AddWithValue("@FenceId", FenceId);
                    await cmd.ExecuteNonQueryAsync();
                }

                // Delete Points
                using (var cmd = new SqlCommand(
                    $"DELETE FROM {(Type == "Circle" ? "FencePointsCircle" : "FencePoints")} WHERE FenceIDFK = @FenceId",
                    con, tran))
                {
                    cmd.Parameters.AddWithValue("@FenceId", FenceId);
                    await cmd.ExecuteNonQueryAsync();
                }

                // Delete Main Fence
                using (var cmd = new SqlCommand(
                    $"DELETE FROM {(Type == "Circle" ? "FenceMainCircle" : "FenceMain")} WHERE ID = @FenceId",
                    con, tran))
                {
                    cmd.Parameters.AddWithValue("@FenceId", FenceId);
                    await cmd.ExecuteNonQueryAsync();
                }

                await tran.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                if (tran != null)
                {
                    try
                    {
                        await tran.RollbackAsync();
                    }
                    catch
                    {

                    }
                }
                return false;
            }
        }

        public async Task<(List<GeoFenceViolation> Data, int TotalCount)> GetGeoFenceViolationReport(
    DataTableRequestModel requestModel,
    string bbid)
          {
            var result = new List<GeoFenceViolation>();
            int totalCount = 0;

            DateTime start = DateTime.Parse(requestModel.beginDate);
            DateTime end = DateTime.Parse(requestModel.endDate);

            string bgdate = start.ToString("yyyy-MM-dd HH:mm:ss");
            string eddate = end.ToString("yyyy-MM-dd HH:mm:ss");

            using var con = new SqlConnection(_connectionString43);
            await con.OpenAsync();

            using var cmd = new SqlCommand("New_TM_GeoFenceVehicle", con);
            cmd.CommandType = CommandType.StoredProcedure;   

            cmd.Parameters.AddWithValue("@LowerBand", requestModel.iDisplayStart);
            cmd.Parameters.AddWithValue("@UpperBand", requestModel.iDisplayStart + requestModel.iDisplayLength);
            cmd.Parameters.AddWithValue("@custId", requestModel.CustId);

            cmd.Parameters.Add("@BBid", SqlDbType.VarChar).Value =
                string.IsNullOrWhiteSpace(bbid) || bbid == "null"
                    ? DBNull.Value
                    : bbid;

            cmd.Parameters.Add("@searchText", SqlDbType.VarChar).Value =
                string.IsNullOrWhiteSpace(requestModel.sSearch) || requestModel.sSearch == "null"
                    ? DBNull.Value
                    : requestModel.sSearch;
            cmd.Parameters.AddWithValue("@sortColumn", requestModel.sortColumn);
            cmd.Parameters.AddWithValue("@sortDirection", requestModel.sortDirection);

            SqlParameter itemCountParam = new SqlParameter("@itemcount", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            cmd.Parameters.Add(itemCountParam);

            var ds = new DataSet();

            using (var adapter = new SqlDataAdapter(cmd))
            {
                adapter.Fill(ds);
            }

            totalCount = itemCountParam.Value != DBNull.Value
                ? Convert.ToInt32(itemCountParam.Value)
                : 0;

            if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            {
                return (new List<GeoFenceViolation>(), totalCount);
            }

            var tasks = ds.Tables[0].AsEnumerable().Select(async vehicleRow =>
            {
                string vehicleBBID = GetString(vehicleRow["bbid"]);

                var vehicle = new GeoFenceViolation
                {
                    VehicleName = GetString(vehicleRow["vehname"]),
                    BBID = vehicleBBID,
                    FenceViolationsCount = 0,
                    Events = new List<GeoFenceViolationDetail>()
                };

                using var alertCon = new SqlConnection(_connectionString43);
                await alertCon.OpenAsync();

                using var alertCmd = new SqlCommand("New_TM_GeoAlert", alertCon);
                alertCmd.CommandType = CommandType.StoredProcedure;

                alertCmd.Parameters.AddWithValue("@custid", requestModel.CustId);
                alertCmd.Parameters.AddWithValue("@bbid", vehicleBBID);
                alertCmd.Parameters.AddWithValue("@date1", bgdate);
                alertCmd.Parameters.AddWithValue("@date2", eddate);

                var alertDs = new DataSet();

                using (var alertAdapter = new SqlDataAdapter(alertCmd))
                {
                    alertAdapter.Fill(alertDs);
                }

                if (alertDs.Tables.Count > 0 && alertDs.Tables[0].Rows.Count > 0)
                {
                    vehicle.FenceViolationsCount = alertDs.Tables[0].Rows.Count;

                    vehicle.Events = alertDs.Tables[0]
                        .AsEnumerable()
                        .Select(row => new GeoFenceViolationDetail
                        {
                            Location = GetString(row["location"]),
                            GeoTime = GetString(row["geotime"]),
                            FenceStatus = GetString(row["fencestatus"]),
                            FenceName = GetString(row["fencename"])
                        })
                        .ToList();
                }

                return vehicle;
            });

            result = (await Task.WhenAll(tasks)).ToList();

            return (result, totalCount);
        }
        public async Task<bool> EditPoi(EditPoiRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString43);
                await con.OpenAsync();

                using var cmd = new SqlCommand("EditPoi", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Action", request.Action);
                cmd.Parameters.AddWithValue("@id", request.Id);

                cmd.Parameters.AddWithValue("@lat", request.Latitude.HasValue ? request.Latitude.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@longi", request.Longitude.HasValue ? request.Longitude.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@details", string.IsNullOrWhiteSpace(request.Details) ? DBNull.Value : request.Details);
                cmd.Parameters.AddWithValue("@radius", string.IsNullOrWhiteSpace(request.Radius) ? DBNull.Value : request.Radius);
                var affectedRows = await cmd.ExecuteNonQueryAsync();

                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Error editing POI: {ex.Message}",
                    ex);
            }
        }
    }
}
