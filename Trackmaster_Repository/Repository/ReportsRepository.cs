using HMSCL.Models;
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
using static Trackmaster_Model.Reports; //added model
using static Trackmaster_Repository.DataTypeHelper;

namespace Trackmaster_Repository.Repository
{
    public class ReportsRepository : IReportsRepository

    {
        private readonly string _connectionString43;
        private readonly string _FMSConString43;
        private readonly string _connectionString44;
        public ReportsRepository(IConfiguration configuration)
        {
            _connectionString43 = configuration.GetConnectionString("DefaultConnection43");
            _FMSConString43 = configuration.GetConnectionString("FMSConString43");
            _connectionString44 = configuration.GetConnectionString("DefaultConnection44");
        }
        public string GetConnectionStringTableWise(string tableName)
        {
            return ((tableName.StartsWith("i", StringComparison.OrdinalIgnoreCase) || tableName.StartsWith("j", StringComparison.OrdinalIgnoreCase)) && tableName.Length > 5) ? _connectionString44 : _connectionString43;
        }

        public VehiclesReport GetConductorInfo(DataTableRequestModel requestModel)
        {
            var modelObj = new VehiclesReport();
            modelObj.modelObjList = new List<VehicleInformation>();
            if (requestModel.sSearch == "null" || string.IsNullOrEmpty(requestModel.sSearch))
            {
                requestModel.sSearch = null;
            }
            if (string.IsNullOrEmpty(requestModel.sortColumn))
                requestModel.sortColumn = "VehName";
            if (string.IsNullOrEmpty(requestModel.sortDirection))
                requestModel.sortDirection = "asc";
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString43))
                {
                    using (SqlCommand cmd = new SqlCommand("[dbo].[GetCrewData]", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@CustId", requestModel.CustId);
                        cmd.Parameters.AddWithValue("@startRowIndex", requestModel.iDisplayStart);
                        cmd.Parameters.AddWithValue("@pageSize", requestModel.iDisplayLength);
                        cmd.Parameters.AddWithValue("@vehName", string.IsNullOrEmpty(requestModel.sSearch) ? (object)DBNull.Value : requestModel.sSearch);
                        cmd.Parameters.AddWithValue("@sortColumn", requestModel.sortColumn);
                        cmd.Parameters.AddWithValue("@sortDirection", requestModel.sortDirection);
                        con.Open();
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);
                            if (dt.Rows.Count == 0)
                                return modelObj;
                            foreach (DataRow dr in dt.Rows)
                            {
                                VehicleInformation objVeh = new VehicleInformation();
                                modelObj.PageCount = GetInt(dr["totalrecords"]);
                                objVeh.BBID = GetString(dr["BBID"]);
                                objVeh.VehicleName = GetString(dr["VehName"]);
                                objVeh.driverName = GetString(dr["DriverName"]);
                                objVeh.ConductorName = GetString(dr["Conductor"]);
                                objVeh.VehicleImagePath = GetString(dr["icon"]);
                                objVeh.VehicleType = GetString(dr["type"]);
                                modelObj.modelObjList.Add(objVeh);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return new VehiclesReport
                {
                    modelObjList = new List<VehicleInformation>()
                };
            }
            return modelObj;
        }

        public List<DropDownItems> GetDesignationTypeCrew()
        {
            List<DropDownItems> lstEmpType = new List<DropDownItems>();
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString43))
                using (SqlCommand cmd = new SqlCommand("[dbo].[GetDesignationTypeCrew]", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                DropDownItems objET = new DropDownItems
                                {
                                    Value = GetInt(dr["EmployeeTypeID"]),
                                    Name = GetString(dr["EmployeeType"])
                                };
                                lstEmpType.Add(objET);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine("SQL Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            return lstEmpType;
        }

        public List<DropDownItems> GetStatesList()
        {
            List<DropDownItems> stateList = new List<DropDownItems>();

            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString43))
                using (SqlCommand cmd = new SqlCommand("[dbo].[GetStatesForCrew]", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    con.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            DropDownItems objET = new DropDownItems
                            {
                                Value = GetInt(dr["PKStateID"]),
                                Name = GetString(dr["StateName"])
                            };

                            stateList.Add(objET);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine("SQL Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            return stateList;
        }

        public List<DropDownItems> GetCityList(int stateid)
        {
            List<DropDownItems> cityList = new List<DropDownItems>();

            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString43))
                using (SqlCommand cmd = new SqlCommand("[dbo].[GetCityForCrew]", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@stateid", stateid);
                    con.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            DropDownItems objET = new DropDownItems
                            {
                                Value = GetInt(dr["PkCityID"]),
                                Name = GetString(dr["CityName"])
                            };

                            cityList.Add(objET);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine("SQL Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            return cityList;
        }

        public string AddUpdateEmployee(Employee objEmp, string imagePaths)
        {
            string result = "";

            try
            {
                using (SqlConnection con = new SqlConnection(_FMSConString43))
                using (SqlCommand cmd = new SqlCommand("[dbo].[EmpInfoAddUpdateCrew]", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Custid", objEmp.Custid);
                    cmd.Parameters.AddWithValue("@EmployeeID", objEmp.EmployeeId); //pk in employee table
                    cmd.Parameters.AddWithValue("@EmployeeTypeId", objEmp.Designation);// designation id
                    cmd.Parameters.AddWithValue("@EmployeeCode", objEmp.EmployeeCode);
                    cmd.Parameters.AddWithValue("@EmployeeType", objEmp.EmployeeType);
                    cmd.Parameters.AddWithValue("@contractDuration", objEmp.contractDuration);
                    cmd.Parameters.AddWithValue("@FirstName", objEmp.FirstName);
                    cmd.Parameters.AddWithValue("@LastName", objEmp.LastName);
                    cmd.Parameters.AddWithValue("@Qualification", objEmp.Qualification);
                    cmd.Parameters.AddWithValue("@Experience", objEmp.Experience);
                    cmd.Parameters.AddWithValue("@PermanentAddress", objEmp.PermanentAddress);
                    cmd.Parameters.AddWithValue("@PermanentPostalCode", objEmp.PermanentPostalCode);
                    cmd.Parameters.AddWithValue("@PermanentState", objEmp.PermanentState);
                    cmd.Parameters.AddWithValue("@PermanentCity", objEmp.PermanentCity);
                    cmd.Parameters.AddWithValue("@Address", objEmp.correspondenceAddress);
                    cmd.Parameters.AddWithValue("@PostalCode", objEmp.correspondencePostalCode);
                    cmd.Parameters.AddWithValue("@State", objEmp.correspondenceState);
                    cmd.Parameters.AddWithValue("@City", objEmp.correspondenceCity);
                    cmd.Parameters.AddWithValue("@HireDate", objEmp.HireDate);
                    cmd.Parameters.AddWithValue("@EmployeeCTC", objEmp.EmployeeCTC);
                    cmd.Parameters.AddWithValue("@role", objEmp.Role);
                    cmd.Parameters.AddWithValue("@OfficePhone", objEmp.OfficePhone);
                    cmd.Parameters.AddWithValue("@EmergencyContactInfo", objEmp.EmergencyContactInfo);
                    cmd.Parameters.AddWithValue("@Mobile", objEmp.Mobile);
                    cmd.Parameters.AddWithValue("@IdProofNo", objEmp.IdProofNo);
                    cmd.Parameters.AddWithValue("@IdProofType", objEmp.IdProofType);
                    cmd.Parameters.AddWithValue("@Remarks", objEmp.Remarks);
                    cmd.Parameters.AddWithValue("@BloodGroup", objEmp.BloodGroup);
                    cmd.Parameters.AddWithValue("@ImagePath", string.IsNullOrEmpty(imagePaths) ? "" : imagePaths);

                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery(); con.Close();
                    result = rowsAffected > 0 ? "Employee saved successfully" : "Failed to save employee";
                    //result = "testing to save employee";
                }
            }
            catch (SqlException ex)
            {
                result = "SQL Error: " + ex.Message;
            }
            catch (Exception ex)
            {
                result = "Error: " + ex.Message;
            }

            return result;
        }





        public VehicleStatusResponse VehicleStatus(int custId, int lower, int upper, string search, DateTime start, DateTime end)
        {
            var result = new VehicleStatusResponse();
            result.VehicleData = new List<VehicleStatusDto>();

            using (SqlConnection con = new SqlConnection(_connectionString43))
            using (SqlCommand cmd = new SqlCommand("NewTMVehicleStatus", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@custId", custId);
                cmd.Parameters.AddWithValue("@LowerBand", lower);
                cmd.Parameters.AddWithValue("@UpperBand", upper);
                cmd.Parameters.AddWithValue("@searchText", (object)search ?? DBNull.Value);

                SqlParameter outParam = new SqlParameter("@ItemCount", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(outParam);

                con.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        result.VehicleData.Add(new VehicleStatusDto
                        {
                            RowNo = Convert.ToInt32(dr["RowNo"]),
                            BBID = dr["BBID"].ToString(),
                            VehName = dr["vehname"].ToString(),
                            DriverName = dr["DriverName"].ToString(),
                            Overspeed = Convert.ToInt32(dr["overspeed"]),
                            Logs = new List<SpeedLogDto>()
                        });
                    }
                }

                result.ItemCount = Convert.ToInt32(outParam.Value);
            }

            foreach (var item in result.VehicleData)
            {
                using (SqlConnection con = new SqlConnection(_connectionString43))
                using (SqlCommand cmd = new SqlCommand("NewGetSummaryDetails", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@bbid", item.BBID);
                    cmd.Parameters.AddWithValue("@overspeed", item.Overspeed);
                    cmd.Parameters.AddWithValue("@beginDate", start);
                    cmd.Parameters.AddWithValue("@EndDate", end);

                    con.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            item.Logs.Add(new SpeedLogDto
                            {
                                Time = Convert.ToDateTime(dr["datadate"]),
                                Speed = dr["speed"].ToString(),
                                Location = dr["loc"].ToString(),
                                Status = dr["status"].ToString()
                            });
                        }
                    }
                }
            }

            return result;
        }
        public async Task<List<DistanceReportDataModel>> GetDistanceReportData(DataTableRequestModel model)
        {
            var result = new List<DistanceReportDataModel>();

            // ================= MAIN DATA =================

            using (SqlConnection con = new SqlConnection(_connectionString43))
            using (SqlCommand cmd = new SqlCommand("GetDistanceReportData", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@custId", model.CustId);
                cmd.Parameters.AddWithValue("@beginDate", model.beginDate);
                cmd.Parameters.AddWithValue("@endDate", model.endDate);
                cmd.Parameters.AddWithValue("@iDisplayStart", model.iDisplayStart);
                cmd.Parameters.AddWithValue("@iDisplayLength", model.iDisplayLength);
                cmd.Parameters.AddWithValue("@sortColumn", model.sortColumn);
                cmd.Parameters.AddWithValue("@sortDirection", model.sortDirection);
                cmd.Parameters.AddWithValue("@sSearch", model.sSearch);

                await con.OpenAsync();

                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        result.Add(new DistanceReportDataModel
                        {
                            Date = GetDateTime(dr["Date"]),
                            BBID = GetString(dr["BBID"]),
                            VehName = GetString(dr["VehName"]),
                            _distanceReportSubDataModel =
                                new List<DistanceReportSubDataModel>()
                        });
                    }
                }
            }

            // ================= PARALLEL DEVICE TABLE CALLS =================

            var tasks = result.Select(async item =>
            {
                var deviceDetailList = new List<PlaybackDataModel>();

                using (SqlConnection con =
                       new SqlConnection(GetConnectionStringTableWise(item.BBID)))
                {
                    await con.OpenAsync();

                    string query = $@"
SELECT speed, datadate, acignition, distance, loc
FROM [{item.BBID}]
WHERE datadate >= @startdate
AND datadate <= @enddate
ORDER BY datadate";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        DateTime startDate = item.Date.Date;
                        DateTime endDate = item.Date.Date
                            .AddDays(1)
                            .AddSeconds(-1);

                        cmd.Parameters.AddWithValue("@startdate", startDate);
                        cmd.Parameters.AddWithValue("@enddate", endDate);

                        using (SqlDataReader dr =
                               await cmd.ExecuteReaderAsync())
                        {
                            while (await dr.ReadAsync())
                            {
                                deviceDetailList.Add(new PlaybackDataModel
                                {
                                    speed = GetInt(dr["speed"]),
                                    datadate = GetDateTime(dr["datadate"]),
                                    acignition =
                                        GetString(dr["acignition"]) == "1"
                                        ? "Off"
                                        : "On",
                                    distance = GetDecimal(dr["distance"]),
                                    location = GetString(dr["loc"])
                                });
                            }
                        }
                    }
                }

                // ================= APPLY DISTANCE LOGIC =================

                bool flag = false;

                decimal sdist = 0;
                decimal edist = 0;

                decimal cumulativeDistance = 0;
                decimal totalDistance = 0;

                DateTime? tripStartTime = null;

                for (int i = 0; i < deviceDetailList.Count; i++)
                {
                    var current = deviceDetailList[i];

                    decimal speed = current.speed;
                    decimal currentDistance = current.distance;

                    // START MOVEMENT
                    if (speed > 0 && flag == false)
                    {
                        sdist = (i == 0)
                            ? currentDistance
                            : deviceDetailList[i - 1].distance;

                        edist = currentDistance;

                        tripStartTime = current.datadate;

                        flag = true;
                    }

                    // CONTINUE MOVEMENT
                    else if (speed > 0 && flag == true)
                    {
                        edist = currentDistance;
                    }

                    // STOP MOVEMENT
                    else if (speed <= 0 && flag == true)
                    {
                        edist = currentDistance;

                        decimal tripDistance =
                            Math.Round(edist - sdist, 1);

                        DateTime tripEndTime = current.datadate;

                        if (tripDistance > 0 && tripDistance < 500)
                        {
                            TimeSpan duration =
                                tripEndTime - tripStartTime.Value;

                            cumulativeDistance += tripDistance;
                            totalDistance += tripDistance;

                            item._distanceReportSubDataModel.Add(
                                new DistanceReportSubDataModel
                                {
                                    StartTime = tripStartTime.Value.ToString("HH:mm"),

                                    EndTime = tripEndTime.ToString("HH:mm"),

                                    Duration = Math.Round(duration.TotalHours, 1).ToString("0.0"),

                                    EstimateDistance = tripDistance.ToString("0.0"),

                                    EstimateCumulativeDistance = cumulativeDistance.ToString("0.0"),

                                    StartLocation = current.location
                                });
                        }

                        flag = false;
                    }
                }

                // HANDLE LAST RUNNING SESSION

                if (flag == true)
                {
                    decimal tripDistance =
                        Math.Round(edist - sdist, 1);

                    DateTime tripEndTime =
                        deviceDetailList.LastOrDefault()?.datadate
                        ?? DateTime.Now;

                    if (tripDistance > 0 && tripDistance < 500)
                    {
                        TimeSpan duration =
                            tripEndTime - tripStartTime.Value;

                        cumulativeDistance += tripDistance;
                        totalDistance += tripDistance;

                        item._distanceReportSubDataModel.Add(
                            new DistanceReportSubDataModel
                            {
                                StartTime = tripStartTime.Value
                                    .ToString("dd-MM-yyyy HH:mm:ss"),

                                EndTime = tripEndTime
                                    .ToString("dd-MM-yyyy HH:mm:ss"),

                                Duration =
                                    duration.ToString(@"hh\:mm\:ss"),

                                EstimateDistance =
                                    tripDistance.ToString("0.0"),

                                EstimateCumulativeDistance =
                                    cumulativeDistance.ToString("0.0"),

                                StartLocation =
                                    deviceDetailList.LastOrDefault()?.location
                            });
                    }
                }

                item.Distance = totalDistance.ToString("0.0");
            });

            // WAIT FOR ALL TABLES TO COMPLETE

            await Task.WhenAll(tasks);

            return result;
        }
    }
}
