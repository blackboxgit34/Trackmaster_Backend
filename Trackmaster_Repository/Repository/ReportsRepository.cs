using HMSCL.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
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
        private readonly string _BlackboxMain_HITEC44; // neha k
        public ReportsRepository(IConfiguration configuration)
        {

            _connectionString43 = configuration.GetConnectionString("DefaultConnection43");
            _FMSConString43 = configuration.GetConnectionString("FMSConString43");
            _connectionString44 = configuration.GetConnectionString("DefaultConnection44");
            _BlackboxMain_HITEC44 = configuration.GetConnectionString("BlackboxMain_HITEC44");
        }
        public string GetConnectionStringTableWise(string tableName)
        {
            return ((tableName.StartsWith("i", StringComparison.OrdinalIgnoreCase) || tableName.StartsWith("j", StringComparison.OrdinalIgnoreCase)) && tableName.Length > 5) ? _connectionString44 : _connectionString43;
        }
        
        public async Task<VehiclesReport> GetConductorInfo(DataTableRequestModel requestModel)
        {
            var modelObj = new VehiclesReport
            {
                modelObjList = new List<VehicleInformation>()
            };
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
                using (SqlCommand cmd = new SqlCommand("[dbo].[GetCrewData]", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CustId", requestModel.CustId);
                    cmd.Parameters.AddWithValue("@startRowIndex", requestModel.iDisplayStart);
                    cmd.Parameters.AddWithValue("@pageSize", requestModel.iDisplayLength);
                    cmd.Parameters.AddWithValue("@vehName",string.IsNullOrEmpty(requestModel.sSearch)? (object)DBNull.Value: requestModel.sSearch);
                    cmd.Parameters.AddWithValue("@sortColumn", requestModel.sortColumn);
                    cmd.Parameters.AddWithValue("@sortDirection", requestModel.sortDirection);
                    await con.OpenAsync();
                    using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                    {
                        while (await dr.ReadAsync())
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
            catch (Exception)
            {
                return new VehiclesReport
                {
                    modelObjList = new List<VehicleInformation>()
                };
            }
            return modelObj;
        }

        public async Task<List<DropDownItems>> GetDesignationTypeCrew()
        {
            List<DropDownItems> lstEmpType = new List<DropDownItems>();
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString43))
                using (SqlCommand cmd = new SqlCommand("[dbo].[GetDesignationTypeCrew]", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    await con.OpenAsync();
                    using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                    {
                        if (dr.HasRows)
                        {
                            while (await dr.ReadAsync())
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

        public async Task<List<DropDownItems>> GetStatesList()
        {
            List<DropDownItems> stateList = new List<DropDownItems>();

            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString43))
                using (SqlCommand cmd = new SqlCommand("[dbo].[GetStatesForCrew]", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    await con.OpenAsync();

                    using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                    {
                        while (await dr.ReadAsync())
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

        public async Task<List<DropDownItems>> GetCityList(int stateid)
        {
            List<DropDownItems> cityList = new List<DropDownItems>();

            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString43))
                using (SqlCommand cmd = new SqlCommand("[dbo].[GetCityForCrew]", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@stateid", stateid);
                    await con.OpenAsync();

                    using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                    {
                        while (await dr.ReadAsync())
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

        public async Task<string> AddUpdateEmployee(Employee objEmp, string imagePaths)
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

                    await con.OpenAsync();
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
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

        public async Task<List<DropDownItems>> GetMessageType()
        {
            var list = new List<DropDownItems>();
            try
            {
                using var con = new SqlConnection(_connectionString43);
                using var cmd = new SqlCommand("GetMessageTypeTM", con);
                cmd.CommandType = CommandType.StoredProcedure;
                await con.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new DropDownItems
                    {
                        Value = GetInt(reader["type_id"]),
                        Name = GetString(reader["type_name"])
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            return list;
        }
        public async Task<SMSReportEx> GetSentMessagesReport(DataTableRequestModel requestModel, int typeid, string messagetype)
        {
            SMSReportEx objSMSReportEx = new SMSReportEx
            {
                objSMSReport = new List<SMSReport>()
            };

            try
            {
                using (SqlConnection con = new SqlConnection(_BlackboxMain_HITEC44))
                using (SqlCommand cmd = new SqlCommand("GetSentSms", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@LowerBand", requestModel.iDisplayStart);
                    cmd.Parameters.AddWithValue("@UpperBand", requestModel.iDisplayLength);

                    SqlParameter itemCountParam = new SqlParameter("@ItemCount", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };

                    cmd.Parameters.Add(itemCountParam);

                    cmd.Parameters.AddWithValue("@MsgType", string.IsNullOrEmpty(messagetype) ? 0 : Convert.ToInt32(messagetype));
                    cmd.Parameters.AddWithValue("@FromDate", requestModel.beginDate);
                    cmd.Parameters.AddWithValue("@ToDate", requestModel.endDate);
                    cmd.Parameters.AddWithValue("@custId", requestModel.CustId);
                    cmd.Parameters.AddWithValue("@type", typeid);
                    cmd.Parameters.AddWithValue("@searchText",
                        string.IsNullOrWhiteSpace(requestModel.sSearch)
                        ? DBNull.Value
                        : requestModel.sSearch);

                    await con.OpenAsync();

                    using (SqlDataReader dsVeh = await cmd.ExecuteReaderAsync())
                    {
                        // Column indexes for better performance
                        int bbidIndex = dsVeh.GetOrdinal("BBID");
                        int fmsVehicleIdIndex = dsVeh.GetOrdinal("FMSVehicleId");
                        //int vehicleNameIndex = dsVeh.GetOrdinal("VehicleName");
                        int sendTimeIndex = dsVeh.GetOrdinal("SendTime");
                        int typeNameIndex = dsVeh.GetOrdinal("type_name");
                        int mobileIndex = dsVeh.GetOrdinal("Mobile");
                        int smsTextIndex = dsVeh.GetOrdinal("SMSText");
                        int androidStatusIndex = dsVeh.GetOrdinal("Androidstatus");
                        int iosStatusIndex = dsVeh.GetOrdinal("iOSstatus");

                        while (await dsVeh.ReadAsync())
                        {
                            SMSReport objSMSReport = new SMSReport
                            {
                                BBID = dsVeh.IsDBNull(bbidIndex) ? string.Empty : dsVeh.GetString(bbidIndex),
                                fmsVehicleId = dsVeh.IsDBNull(fmsVehicleIdIndex) ? 0 : Convert.ToInt32(dsVeh[fmsVehicleIdIndex]),
                                //VehicleName = dsVeh.IsDBNull(vehicleNameIndex) ? string.Empty : dsVeh.GetString(vehicleNameIndex),
                                MessageDate = dsVeh.IsDBNull(sendTimeIndex) ? string.Empty : Convert.ToString(dsVeh[sendTimeIndex]),
                                MessageType = dsVeh.IsDBNull(typeNameIndex) ? string.Empty : dsVeh.GetString(typeNameIndex),
                                Mobile = dsVeh.IsDBNull(mobileIndex) ? string.Empty : dsVeh.GetString(mobileIndex),
                                MessageText = dsVeh.IsDBNull(smsTextIndex) ? string.Empty : dsVeh.GetString(smsTextIndex),
                                androidstatus = dsVeh.IsDBNull(androidStatusIndex) ? string.Empty : dsVeh.GetString(androidStatusIndex),
                                iosstatus = dsVeh.IsDBNull(iosStatusIndex) ? string.Empty : dsVeh.GetString(iosStatusIndex)
                            };

                            objSMSReportEx.objSMSReport.Add(objSMSReport);
                        }
                    }

                    objSMSReportEx.pagecount = itemCountParam.Value != DBNull.Value
                        ? Convert.ToInt32(itemCountParam.Value)
                        : 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);

                return new SMSReportEx
                {
                    objSMSReport = new List<SMSReport>(),
                    pagecount = 0
                };
            }

            return objSMSReportEx;
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
                            RowNo = GetInt(dr["RowNo"]),
                            BBID = GetString(dr["BBID"]),
                            VehName = GetString(dr["vehname"]),
                            DriverName = GetString(dr["DriverName"]),
                            Overspeed = GetInt(dr["overspeed"]),
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
                                Time = GetDateTime(dr["datadate"]),
                                Speed = GetString(dr["speed"]),
                                Location = GetString(dr["loc"]),
                                Status = GetString(dr["status"])
                            });
                        }
                    }
                }
            }

            return result;
        }

        public StoppageMainModel GetCombinedStoppageReport(DataTableRequestModel dtmodel)
        {
            var model = new StoppageMainModel
            {
                StoppageSubModel = new List<StoppageSubModel>()
            };

            var intervalParts = dtmodel.Interval.Split('-');
            int minInterval = Convert.ToInt32(intervalParts[0]);
            int maxInterval = Convert.ToInt32(intervalParts[1]);

            Dictionary<string, string> poiCache = new Dictionary<string, string>();

            using (SqlConnection con = new SqlConnection(_connectionString43))
            {
                con.Open();

                #region Vehicle List

                SqlParameter[] param =
                {
            new SqlParameter("@LowerBand", dtmodel.iDisplayStart),
            new SqlParameter("@UpperBand", dtmodel.iDisplayLength),
            new SqlParameter("@ItemCount", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            },
            new SqlParameter("@custId", dtmodel.CustId),
            new SqlParameter("@searchText",
                string.IsNullOrWhiteSpace(dtmodel.sSearch)
                    ? (object)DBNull.Value
                    : dtmodel.sSearch)
        };

                List<string> bbidList = new List<string>();

                using (SqlCommand cmd = new SqlCommand("GetVehiclesByCustIdAndSearch", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddRange(param);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string bbid = reader["bbid"]?.ToString();

                            if (!string.IsNullOrEmpty(bbid))
                                bbidList.Add(bbid);
                        }
                    }
                }

                model.PageCount = Convert.ToInt32(param[2].Value);

                if (!bbidList.Any())
                    return model;

                #endregion

                #region Get Vehicle Master Data In Single Query

                string bbidCsv = string.Join(",", bbidList.Select(x => $"'{x}'"));

                string vehicleQuery = $@"
            SELECT 
                bbid,
                vehname,
                ISNULL(DriverName,'Not available') AS DriverName,
                distance
            FROM ht_main WITH(NOLOCK)
            WHERE bbid IN ({bbidCsv})";

                Dictionary<string, dynamic> vehicleData = new Dictionary<string, dynamic>();

                using (SqlCommand cmd = new SqlCommand(vehicleQuery, con))
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        vehicleData[dr["bbid"].ToString()] = new
                        {
                            VehicleName = dr["vehname"]?.ToString(),
                            DriverName = dr["DriverName"]?.ToString(),
                            Distance = dr["distance"]?.ToString()
                        };
                    }
                }

                #endregion

                foreach (string vehicleId in bbidList)
                {
                    var stoppageSubModel = new StoppageSubModel();
                    var stoppageList = new List<StoppageAnalysis>();

                    if (vehicleData.ContainsKey(vehicleId))
                    {
                        var vehicle = vehicleData[vehicleId];

                        stoppageSubModel.BBID = vehicleId;
                        stoppageSubModel.VehicleName = vehicle.VehicleName;
                        stoppageSubModel.DriverName = string.IsNullOrWhiteSpace(vehicle.DriverName)
                            ? "Not available"
                            : vehicle.DriverName;
                    }

                    DataTable dt = StoppageAnalysis(Convert.ToDateTime(dtmodel.beginDate), Convert.ToDateTime(dtmodel.endDate), vehicleId, "normal");

                    if (dt == null || dt.Rows.Count == 0)
                    {
                        stoppageSubModel.TotalStoppageTime =
                            "0 day(s) 0 hour(s) 0 minute(s) 0 second(s)";

                        model.StoppageSubModel.Add(stoppageSubModel);
                        continue;
                    }

                    long totalSeconds = 0;

                    foreach (DataRow row in dt.Rows)
                    {
                        double dur = row["floatdur"] == DBNull.Value
                            ? 0
                            : Convert.ToDouble(row["floatdur"]);

                        if (dur <= 0)
                            continue;

                        bool ignition = row["acignition"] != DBNull.Value &&
                                        Convert.ToBoolean(row["acignition"]);

                        bool includeRecord = false;

                        if (minInterval > 0)
                        {
                            if (minInterval >= 600)
                            {
                                includeRecord = dur >= minInterval;
                            }
                            else
                            {
                                includeRecord = dur >= minInterval &&
                                                dur <= maxInterval;
                            }
                        }
                        else
                        {
                            includeRecord = true;
                        }

                        if (!includeRecord)
                            continue;

                        var item = new StoppageAnalysis
                        {
                            IgnitionStatus = ignition,
                            StopLatitude = row["slat"]?.ToString(),
                            StopLongitude = row["slong"]?.ToString(),
                            StartDate = row["sdt"]?.ToString(),
                            StopDate = row["edt"]?.ToString(),
                            Duration = row["duration"]?.ToString(),
                            VehicleName = stoppageSubModel.VehicleName
                        };

                        string location = row["sloc"]?.ToString();

                        item.StopLocation = location;

                        #region POI Cache

                        string poiKey = $"{item.StopLatitude}_{item.StopLongitude}";

                        if (!poiCache.ContainsKey(poiKey))
                        {
                            poiCache[poiKey] = GetPoiLoc(
                                Convert.ToDouble(item.StopLatitude),
                                Convert.ToDouble(item.StopLongitude),
                                dtmodel.CustId);
                        }

                        item.poi = poiCache[poiKey] == "0 Km  of "
                            ? "N/A"
                            : poiCache[poiKey];

                        #endregion

                        if (!string.IsNullOrWhiteSpace(location) &&
                            location != "N/A" &&
                            location != "GPS signal not available")
                        {
                            string status = ignition
                                ? "~/resources/images/legends/stop.png"
                                : "~/resources/images/legends/ignion.png";

                            item.AddPoi =
                                $"<a href='/Common/ADDPOI?lat={item.StopLatitude}&longi={item.StopLongitude}' target='_blank'>Add POI</a>";

                            //item.StopLocation =
                            //    $"<a href='javascript:' onclick=showMapWindow('{vehicleId}','{stoppageSubModel.VehicleName}','{item.StopLatitude}','{item.StopLongitude}','{location}','{status}');>{location}</a>";
                        }
                        else
                        {
                            item.AddPoi = "N/A";
                        }

                        stoppageList.Add(item);

                        totalSeconds += Convert.ToInt64(dur);
                    }

                    TimeSpan total = TimeSpan.FromSeconds(totalSeconds);

                    stoppageSubModel.Type = "0";
                    stoppageSubModel.StoppageCount = stoppageList.Count;
                    stoppageSubModel.TotalStoppageTime =
                        $"{total.Days} day(s) {total.Hours} hour(s) {total.Minutes} minute(s) {total.Seconds} second(s)";
                    stoppageSubModel.objStoppageReport = stoppageList;

                    model.StoppageSubModel.Add(stoppageSubModel);
                }
            }

            return model;
        }

        public DataTable StoppageAnalysis(DateTime beginDate, DateTime endDate, string vehicleId, string mode)
        {
            string p1 = beginDate.ToString("yyyy.MM.dd HH:mm:ss");
            string p2 = endDate.ToString("yyyy.MM.dd HH:mm:ss");
            try
            {
                int Count = 1;
                int first = 1;
                long sum = 0;
                int coun = -1;
                int rowcount = 0;
                string Preplace = string.Empty;
                string Curplace = string.Empty;
                string endtime = string.Empty;
                System.DateTime startdt = default(System.DateTime);
                System.DateTime enddt = default(System.DateTime);
                SqlCommand cmd = new SqlCommand();
                System.Data.DataTable objDT = null;
                System.Data.DataRow objDR = null;
                objDT = new System.Data.DataTable("stoppage");
                objDT.Columns.Add("startdate", typeof(string));
                objDT.Columns.Add("sdt", typeof(string));
                objDT.Columns.Add("edt", typeof(string));
                objDT.Columns.Add("enddate", typeof(string));
                objDT.Columns.Add("dist", typeof(double));
                objDT.Columns.Add("duration", typeof(string));
                objDT.Columns.Add("sloc", typeof(string));
                objDT.Columns.Add("slat", typeof(string));
                objDT.Columns.Add("slong", typeof(string));
                objDT.Columns.Add("eloc", typeof(string));
                objDT.Columns.Add("acignition", typeof(bool));
                objDT.Columns.Add("fuelLevel", typeof(float));
                objDT.Columns.Add("floatdur", typeof(float));
                objDT.Columns.Add("ignitionOn", typeof(bool));

                string query = @"SELECT overstop,
                        overstop_Min,
                        box
                 FROM ht_main WITH (NOLOCK)
                 WHERE bbid = @BBID";

                int ostop = 0;
                int ostop_Min = 0;
                string boxType = string.Empty;

                using (SqlConnection con12 = new SqlConnection(_connectionString43))
                {
                    con12.Open();

                    using (SqlCommand cmd1 = new SqlCommand(query, con12))
                    {
                        cmd1.CommandType = CommandType.Text;

                        cmd1.Parameters.AddWithValue("@BBID", vehicleId);

                        using (SqlDataReader dr5 = cmd1.ExecuteReader())
                        {
                            if (dr5.Read())
                            {
                                ostop = dr5["overstop"] == DBNull.Value
                                    ? 0
                                    : Convert.ToInt32(dr5["overstop"]);

                                ostop_Min = dr5["overstop_Min"] == DBNull.Value
                                    ? 0
                                    : Convert.ToInt32(dr5["overstop_Min"]);

                                boxType = dr5["box"] == DBNull.Value
                                    ? string.Empty
                                    : dr5["box"].ToString();
                            }
                        }
                    }
                }
                DataSet dr4 = new DataSet();

                string query1 = $@"SELECT distance,
                         speed,
                         datadate,
                         acignition,
                         vehBatVoltage,
                         loc,
                         latitude,
                         longitude,
                         fuelLevel
                  FROM [{vehicleId}] WITH (NOLOCK)
                  WHERE datadate >= @P1
                    AND datadate <= @P2
                  ORDER BY datadate ASC";

                using (SqlConnection con = new SqlConnection(GetConnectionStringTableWise(vehicleId)))
                {
                    con.Open();

                    using (SqlCommand cmd3 = new SqlCommand(query1, con))
                    {
                        cmd3.CommandType = CommandType.Text;

                        cmd3.Parameters.AddWithValue("@P1", p1);
                        cmd3.Parameters.AddWithValue("@P2", p2);

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd3))
                        {
                            da.Fill(dr4);
                        }
                    }
                    //DataSet dr4 = new DataSet();
                    //dr4 = SqlHelper.ExecuteDataset(Utility.GetConnectionString(vehicleId), CommandType.Text, "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;select distance, speed,datadate,acignition,vehBatVoltage,loc,latitude,longitude,fuelLevel from " + vehicleId + " where datadate >= '" + p1 + "' and datadate <= '" + p2 + "' order by datadate asc");
                    bool flag = false;
                    System.DateTime startd = default(System.DateTime);
                    System.DateTime endd = default(System.DateTime);
                    long mdate = 0;
                    long testdate = 0;
                    long mhour = 0;
                    long mmin = 0;
                    int stopalrm = 0;
                    if (mode == "normal")
                    {
                        if (dr4.Tables[0].Rows.Count > 0)
                        {
                            for (int iCount = 0; iCount <= dr4.Tables[0].Rows.Count - 1; iCount++)
                            {

                                if (Convert.ToInt32(dr4.Tables[0].Rows[iCount]["speed"]) == 0 & Convert.ToInt32(dr4.Tables[0].Rows[iCount]["acignition"]) == 1 & flag == false)
                                {
                                    objDR = objDT.NewRow();
                                    objDR["startdate"] = Convert.ToDateTime(dr4.Tables[0].Rows[iCount]["datadate"]);
                                    objDR["sloc"] = Convert.ToString(dr4.Tables[0].Rows[iCount]["loc"]);
                                    objDR["slat"] = Convert.ToString(dr4.Tables[0].Rows[iCount]["latitude"]);
                                    objDR["slong"] = Convert.ToString(dr4.Tables[0].Rows[iCount]["longitude"]);
                                    objDR["dist"] = Convert.ToString(dr4.Tables[0].Rows[iCount]["distance"]);
                                    objDR["acignition"] = Convert.ToBoolean(dr4.Tables[0].Rows[iCount]["acignition"]);
                                    objDR["sdt"] = Convert.ToDateTime(dr4.Tables[0].Rows[iCount]["datadate"]).ToString("dd/MMM/yyyy hh:mm:ss  tt");
                                    startd = Convert.ToDateTime(dr4.Tables[0].Rows[iCount]["datadate"]);
                                    flag = true;
                                }

                                else if (Convert.ToInt32(dr4.Tables[0].Rows[iCount]["speed"]) > 0 & Convert.ToInt32(dr4.Tables[0].Rows[iCount]["acignition"]) == 1 & flag == false)
                                {
                                    objDR = objDT.NewRow();
                                    objDR["startdate"] = Convert.ToDateTime(dr4.Tables[0].Rows[iCount]["datadate"]);
                                    objDR["sloc"] = Convert.ToString(dr4.Tables[0].Rows[iCount]["loc"]);
                                    objDR["slat"] = Convert.ToString(dr4.Tables[0].Rows[iCount]["latitude"]);
                                    objDR["slong"] = Convert.ToString(dr4.Tables[0].Rows[iCount]["longitude"]);
                                    objDR["dist"] = Convert.ToString(dr4.Tables[0].Rows[iCount]["distance"]);
                                    objDR["acignition"] = Convert.ToBoolean(dr4.Tables[0].Rows[iCount]["acignition"]);
                                    objDR["sdt"] = Convert.ToDateTime(dr4.Tables[0].Rows[iCount]["datadate"]).ToString("dd/MMM/yyyy hh:mm:ss  tt");
                                    startd = Convert.ToDateTime(dr4.Tables[0].Rows[iCount]["datadate"]);
                                    flag = true;
                                }
                                else if (Convert.ToInt32(dr4.Tables[0].Rows[iCount]["speed"]) > 0 & Convert.ToInt32(dr4.Tables[0].Rows[iCount]["acignition"]) == 1 & flag == true)
                                {
                                    endd = Convert.ToDateTime(dr4.Tables[0].Rows[iCount]["datadate"]);
                                }
                                else if (Convert.ToInt32(dr4.Tables[0].Rows[iCount]["speed"]) == 0 & Convert.ToInt32(dr4.Tables[0].Rows[iCount]["acignition"]) == 1 & flag == true)
                                {
                                    endd = Convert.ToDateTime(dr4.Tables[0].Rows[iCount]["datadate"]);
                                }



                                else if (Convert.ToInt32(dr4.Tables[0].Rows[iCount]["speed"]) > 0 & Convert.ToInt32(dr4.Tables[0].Rows[iCount]["acignition"]) == 0 & flag == true)
                                {
                                    if (endd < Convert.ToDateTime(dr4.Tables[0].Rows[iCount]["datadate"]))
                                    {
                                        objDR["enddate"] = Convert.ToDateTime(dr4.Tables[0].Rows[iCount]["datadate"]);
                                        objDR["edt"] = Convert.ToDateTime(dr4.Tables[0].Rows[iCount]["datadate"]).ToString("dd/MMM/yyyy hh:mm:ss  tt");
                                        endd = Convert.ToDateTime(dr4.Tables[0].Rows[iCount]["datadate"]);
                                    }
                                    else
                                    {
                                        objDR["enddate"] = endd;
                                        objDR["edt"] = Convert.ToDateTime(endd).ToString("dd/MMM/yyyy hh:mm:ss  tt");
                                    }

                                    objDR["eloc"] = Convert.ToString(dr4.Tables[0].Rows[iCount]["loc"]);
                                    TimeSpan ts = endd.Subtract(startd);
                                    mdate = Convert.ToInt32(ts.TotalSeconds);
                                    testdate = testdate + mdate;
                                    mhour = ts.Hours;
                                    objDR["duration"] = GetElapsedTime1(mdate);
                                    objDR["floatdur"] = mdate;
                                    //if (Count == 1)
                                    //{
                                    Preplace = Convert.ToString(dr4.Tables[0].Rows[iCount]["loc"]);
                                    objDT.Rows.Add(objDR);
                                    startdt = Convert.ToDateTime(objDR["sdt"]);
                                    coun++;

                                    Count = 2;
                                    endtime = string.Empty;

                                    if (mhour >= ostop)
                                    {
                                        stopalrm = stopalrm + 1;
                                    }
                                    flag = false;
                                }

                                else if (Convert.ToInt32(dr4.Tables[0].Rows[iCount]["speed"]) == 0 & Convert.ToInt32(dr4.Tables[0].Rows[iCount]["acignition"]) == 0 & flag == true)
                                {
                                    if (endd < Convert.ToDateTime(dr4.Tables[0].Rows[iCount]["datadate"]))
                                    {
                                        objDR["enddate"] = Convert.ToDateTime(dr4.Tables[0].Rows[iCount]["datadate"]);
                                        objDR["edt"] = Convert.ToDateTime(dr4.Tables[0].Rows[iCount]["datadate"]).ToString("dd/MMM/yyyy hh:mm:ss  tt");
                                        endd = Convert.ToDateTime(dr4.Tables[0].Rows[iCount]["datadate"]);
                                    }
                                    else
                                    {
                                        objDR["enddate"] = endd;
                                        objDR["edt"] = Convert.ToDateTime(endd).ToString("dd/MMM/yyyy hh:mm:ss  tt");
                                    }

                                    objDR["eloc"] = Convert.ToString(dr4.Tables[0].Rows[iCount]["loc"]);
                                    TimeSpan ts = endd.Subtract(startd);
                                    mdate = Convert.ToInt32(ts.TotalSeconds);
                                    testdate = testdate + mdate;
                                    mhour = ts.Hours;
                                    objDR["duration"] = GetElapsedTime1(mdate);
                                    objDR["floatdur"] = mdate;
                                    //if (Count == 1)
                                    //{
                                    Preplace = Convert.ToString(dr4.Tables[0].Rows[iCount]["loc"]);
                                    objDT.Rows.Add(objDR);
                                    startdt = Convert.ToDateTime(objDR["sdt"]);
                                    coun++;

                                    Count = 2;
                                    endtime = string.Empty;

                                    if (mhour >= ostop)
                                    {
                                        stopalrm = stopalrm + 1;
                                    }
                                    flag = false;
                                }


                            }

                            if (flag == true)
                            {
                                TimeSpan ts = endd.Subtract(startd);
                                mdate = Convert.ToInt32(ts.TotalSeconds);
                                testdate = testdate + mdate;
                                if (mdate > 0)
                                {
                                    objDR["duration"] = GetElapsedTime1(mdate);
                                    objDR["floatdur"] = mdate;
                                }
                                objDT.Rows.Add(objDR);
                            }
                        }
                    }

                    string testd = null;
                    testd = GetElapsedTime1(testdate);
                    if (objDT.Rows.Count > 0)
                    {
                        return objDT;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return null;
        }

        private static string GetElapsedTime1(long interval)
        {
            String functionReturnValue = null;
            try
            {
                long totalhours = 0;
                long totalminutes = 0;
                long totalseconds = 0;
                long days = 0;
                long hours = 0;
                long Minutes = 0;
                long Seconds = 0;
                days = Convert.ToInt32(Convert.ToSingle(interval / 86400));
                totalhours = Convert.ToInt32(Convert.ToSingle(interval / 3600));
                totalminutes = Convert.ToInt32(Convert.ToSingle(interval / 60));
                totalseconds = Convert.ToInt32(Convert.ToSingle(interval));
                hours = totalhours % 24;
                Minutes = totalminutes % 60;
                Seconds = totalseconds % 60;
                string dayT = days.ToString();
                string hourT = hours.ToString();
                string minT = Minutes.ToString();
                string secT = Seconds.ToString();
                if (dayT.Length == 1)
                {
                    dayT = "0" + dayT;
                }
                if (hourT.Length == 1)
                {
                    hourT = "0" + hourT;
                }
                if (minT.Length == 1)
                {
                    minT = "0" + minT;
                }
                if (secT.Length == 1)
                {
                    secT = "0" + secT;
                }
                functionReturnValue = dayT + "-" + hourT + ":" + minT + ":" + secT;
            }
            catch (Exception ex)
            {
            }
            return functionReturnValue;
        }


        public string GetPoiLoc(double lat, double longi, int custid)
        {
            string tempdirection = string.Empty;

            string functionReturnValue = null;
            double latlow = default(double);
            double latupper = default(double);
            double longilow = default(double);
            double longiupper = default(double);
            double dist = default(double);
            double tempdist = default(double);
            double tdist = default(double);
            string city = null;
            //string state = null;
            bool flag = false;

            Int32 i = default(Int32);
            try
            {
                dist = 0;
                latupper = lat + 0.02;
                latlow = lat - 0.02;
                longiupper = longi + 0.02;
                longilow = longi - 0.02;
                flag = true;
                DataSet ds = new DataSet();
                //string strqry = "select * from latlong1 where lat > " + latlow + " and lat < " + latupper + " and longi > " + longilow + " and longi < " + longiupper + "";

                using (SqlConnection sqlcon = new SqlConnection(_connectionString43))
                {
                    SqlParameter[] param = new SqlParameter[]
                    {
                         new SqlParameter("@latLow", latlow),
                         new SqlParameter("@latUpper", latupper),
                         new SqlParameter("@longiLow", longilow),
                         new SqlParameter("@longiUpper", longiupper),
                         new SqlParameter("@custid", custid)
                    };
                    SqlCommand cmd = new SqlCommand("[dbo].[CustLatLong1]", sqlcon);
                    cmd.Parameters.AddRange(param);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(ds);
                    }
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        for (i = 0; i <= ds.Tables[0].Rows.Count - 1; i++)
                        {
                            tempdist = Math.Sqrt(Math.Pow((Convert.ToDouble(Convert.ToDouble(ds.Tables[0].Rows[i]["lat"])) - lat), 2) + Math.Pow((Convert.ToDouble(ds.Tables[0].Rows[i]["longi"]) - longi), 2)) * 100;
                            if (dist == 0)
                            {
                                if (Convert.ToDouble(ds.Tables[0].Rows[i]["lat"]) > lat)
                                {
                                    tempdirection = " S ";
                                }
                                else
                                {
                                    tempdirection = " N ";
                                }

                                if (Convert.ToDouble(ds.Tables[0].Rows[i]["longi"]) > longi)
                                {
                                    tempdirection = tempdirection + " W ";
                                }
                                else
                                {
                                    tempdirection = tempdirection + " E ";
                                }

                                dist = tempdist;
                                tdist = dist;
                                city = Convert.ToString(ds.Tables[0].Rows[i]["details"]);
                                //state = Convert.ToString(ds.Tables[0].Rows[i]["state"]);
                            }
                            else
                            {
                                if (dist > tempdist)
                                {
                                    dist = tempdist;
                                    tdist = tempdist;
                                    city = Convert.ToString(ds.Tables[0].Rows[i]["details"]);
                                    //state = Convert.ToString(ds.Tables[0].Rows[i]["state"]);
                                    if (Convert.ToDouble(ds.Tables[0].Rows[i]["lat"]) > lat)
                                    {
                                        tempdirection = " S ";
                                    }
                                    else
                                    {
                                        tempdirection = " N ";
                                    }

                                    if (Convert.ToDouble(ds.Tables[0].Rows[i]["longi"]) > longi)
                                    {
                                        tempdirection = tempdirection + " W ";
                                    }
                                    else
                                    {
                                        tempdirection = tempdirection + " E ";
                                    }
                                }
                            }
                        }
                        functionReturnValue = Convert.ToString(Math.Round(tdist, 2)) + " Km " + tempdirection + " of " + city;
                    }
                    else if (ds.Tables[0].Rows.Count == 0)
                    {
                        latupper = lat + 0.09;
                        latlow = lat - 0.09;
                        longiupper = longi + 0.09;
                        longilow = longi - 0.09;

                        SqlParameter[] param2 = new SqlParameter[]
                        {
                            new SqlParameter("@latLow", latlow),
                            new SqlParameter("@latUpper", latupper),
                            new SqlParameter("@longiLow", longilow),
                            new SqlParameter("@longiUpper", longiupper),
                            new SqlParameter("@custid", custid)
                        };

                        DataSet ds1 = new DataSet();
                        SqlCommand cmd6 = new SqlCommand("[dbo].[CustLatLong1]", sqlcon);
                        cmd6.Parameters.AddRange(param);
                        cmd6.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd6))
                        {
                            da.Fill(ds1);
                        }
                        if (ds1.Tables[0].Rows.Count > 0)
                        {
                            flag = false;
                            for (i = 0; i <= ds1.Tables[0].Rows.Count - 1; i++)
                            {
                                tempdist = Math.Sqrt(Math.Pow((Convert.ToDouble(ds1.Tables[0].Rows[i]["lat"]) - lat), 2) + Math.Pow((Convert.ToDouble(ds1.Tables[0].Rows[i]["longi"]) - longi), 2)) * 100;
                                if (dist == 0)
                                {
                                    if (Convert.ToDouble(ds1.Tables[0].Rows[i]["lat"]) > lat)
                                    {
                                        tempdirection = " S ";
                                    }
                                    else
                                    {
                                        tempdirection = " N ";
                                    }

                                    if (Convert.ToDouble(ds1.Tables[0].Rows[i]["longi"]) > longi)
                                    {
                                        tempdirection = tempdirection + " W ";
                                    }
                                    else
                                    {
                                        tempdirection = tempdirection + " E ";
                                    }
                                    dist = tempdist;
                                    tdist = dist;
                                    city = Convert.ToString(ds1.Tables[0].Rows[i]["details"]);
                                    //state = Convert.ToString(ds1.Tables[0].Rows[i]["state"]);
                                }
                                else
                                {
                                    if (dist > tempdist)
                                    {
                                        dist = tempdist;
                                        tdist = tempdist;
                                        city = Convert.ToString(ds1.Tables[0].Rows[i]["details"]);
                                        //state = Convert.ToString(ds1.Tables[0].Rows[i]["state"]);
                                        if (Convert.ToDouble(ds1.Tables[0].Rows[i]["lat"]) > lat)
                                        {
                                            tempdirection = " S ";
                                        }
                                        else
                                        {
                                            tempdirection = " N ";
                                        }
                                        if (Convert.ToDouble(ds1.Tables[0].Rows[i]["longi"]) > longi)
                                        {
                                            tempdirection = tempdirection + " W ";
                                        }
                                        else
                                        {
                                            tempdirection = tempdirection + " E ";
                                        }
                                    }
                                }
                            }
                        }
                        functionReturnValue = Convert.ToString(Math.Round(tdist, 2)) + " Km " + tempdirection + " of " + city;
                    }
                    else
                    {
                        flag = false;
                        functionReturnValue = "N/A";
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return functionReturnValue;
        }
        public async Task<(List<DistanceReportDataModel> data,int TotalCount)> GetDistanceReportData(DataTableRequestModel model)
        {
            var result = new List<DistanceReportDataModel>();
            int TotalCount = 0;
            try
            {

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

                    SqlParameter totalCountParam = new SqlParameter("@TotalCount", SqlDbType.Int);
                    totalCountParam.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(totalCountParam);

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
                    TotalCount = Convert.ToInt32(totalCountParam.Value);
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
                                    StartTime = tripStartTime.Value.ToString("HH:mm"),

                                    EndTime = tripEndTime.ToString("HH:mm"),

                                    Duration = Math.Round(duration.TotalHours, 1).ToString("0.0"),

                                    EstimateDistance = tripDistance.ToString("0.0"),

                                    EstimateCumulativeDistance = cumulativeDistance.ToString("0.0"),

                                    StartLocation = deviceDetailList.LastOrDefault()?.location
                                });
                        }
                    }

                    item.Distance = totalDistance.ToString("0.0");
                });

                // WAIT FOR ALL TABLES TO COMPLETE

                await Task.WhenAll(tasks);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            return (result,TotalCount);
        }
        
    }
}
