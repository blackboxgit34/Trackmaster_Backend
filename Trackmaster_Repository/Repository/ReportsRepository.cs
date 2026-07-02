using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Trackmaster_Model;
using Trackmaster_Repository.Interface;
using static Trackmaster_Model.Reports; //added model
using static Trackmaster_Repository.DataTypeHelper;
using static Trackmaster_Repository.SqlHelper;

namespace Trackmaster_Repository.Repository
{
    public class ReportsRepository : IReportsRepository

    {
        private readonly string _connectionString43;
        private readonly string _FMSConString43;
        private readonly string _connectionString44;
        private readonly string _BlackboxMain_HITEC44; // neha k
        private readonly string _defaultConnectionOrange44;
        public ReportsRepository(IConfiguration configuration)
        {

            _connectionString43 = configuration.GetConnectionString("DefaultConnection43");
            _FMSConString43 = configuration.GetConnectionString("FMSConString43");
            _connectionString44 = configuration.GetConnectionString("DefaultConnection44");
            _BlackboxMain_HITEC44 = configuration.GetConnectionString("BlackboxMain_HITEC44");
            _defaultConnectionOrange44 = configuration.GetConnectionString("DefaultConnectionOrange44");
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
                    cmd.Parameters.AddWithValue("@vehName", string.IsNullOrEmpty(requestModel.sSearch) ? (object)DBNull.Value : requestModel.sSearch);
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
        public async Task<SMSReportEx> GetSentMessagesReport(DataTableRequestModel requestModel, int typeid, string messagetype, string vehicleNo)
        {
            SMSReportEx objSMSReportEx = new SMSReportEx
            {
                objSMSReport = new List<SMSReport>()
            };

            try
            {
                using (SqlConnection con = new SqlConnection(_BlackboxMain_HITEC44))
                using (SqlCommand cmd = new SqlCommand("GetSentSms_TM", con))
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
                    cmd.Parameters.Add("@FromDate", SqlDbType.DateTime).Value = DateTime.Parse(requestModel.beginDate);
                    cmd.Parameters.Add("@ToDate", SqlDbType.DateTime).Value = DateTime.Parse(requestModel.endDate);
                    cmd.Parameters.AddWithValue("@custId", requestModel.CustId);
                    cmd.Parameters.AddWithValue("@type", typeid);
                    cmd.Parameters.AddWithValue("@searchText", string.IsNullOrWhiteSpace(requestModel.sSearch) ? DBNull.Value : requestModel.sSearch);
                    cmd.Parameters.AddWithValue("@vehicleNo", string.IsNullOrWhiteSpace(vehicleNo) ? DBNull.Value : vehicleNo);// neha k 
                    await con.OpenAsync();
                    using (SqlDataReader dsVeh = await cmd.ExecuteReaderAsync())
                    {
                        // Column indexes for better performance
                        int bbidIndex = dsVeh.GetOrdinal("BBID");
                        int fmsVehicleIdIndex = dsVeh.GetOrdinal("FMSVehicleId");
                        int vehicleNameIndex = dsVeh.GetOrdinal("VehicleName");
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
                                VehicleName = dsVeh.IsDBNull(vehicleNameIndex) ? string.Empty : dsVeh.GetString(vehicleNameIndex),
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
        //neha k

        public async Task<ConsolidatedIgnitionModel> GetConsolidatedIgnitionStatus(DataTableRequestModel requestModel, string bbid, string reportName)
        {
            var model = new ConsolidatedIgnitionModel
            {
                ConsolidatedIgnitionList = new List<IgnitionStatusEx>()
            };
            int TotalCount = 0;
            try
            {
                // ================= MAIN DATA =================
                using (SqlConnection con = new SqlConnection(_connectionString43))
                using (SqlCommand cmd = new SqlCommand("GetVehiclesByCustIdAndSearch", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@custId", requestModel.CustId);
                    cmd.Parameters.AddWithValue("@iDisplayStart", requestModel.iDisplayStart);
                    cmd.Parameters.AddWithValue("@iDisplayLength", requestModel.iDisplayLength);
                    cmd.Parameters.AddWithValue("@sortColumn", requestModel.sortColumn);
                    cmd.Parameters.AddWithValue("@sortDirection", requestModel.sortDirection);
                    cmd.Parameters.AddWithValue("@sSearch", requestModel.sSearch);
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
                            model.ConsolidatedIgnitionList.Add(new IgnitionStatusEx
                            {
                                bbid = GetString(dr["BBID"]),
                                VehicleName = GetString(dr["VehName"]),
                                DriverName = GetString(dr["DriverName"]),
                                custid = requestModel.CustId,
                                IgnitionOnOffCounter = "0",
                                TotalIgnitionTime = "0 Day(s)-0 Hour(s):0 Minute(s):0 Second(s)",
                                objIgnitionStatusReport = new List<IgnitionStatus>()
                            });
                        }
                    }
                    TotalCount = totalCountParam.Value != DBNull.Value ? Convert.ToInt32(totalCountParam.Value) : 0;
                }
                // ================= PARALLEL DEVICE TABLE CALLS =================
                var tasks = model.ConsolidatedIgnitionList.Select(async item =>
                {
                    var deviceDetailList = new List<PlaybackDataModel>();
                    using (SqlConnection con = new SqlConnection(GetConnectionStringTableWise(item.bbid)))
                    {
                        await con.OpenAsync();
                        string query = $@" SELECT speed,datadate,acignition,distance,loc FROM [{item.bbid}] WHERE datadate >= @startdate AND datadate <= @enddate ORDER BY datadate ASC";
                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            DateTime startDate = GetDateTime(requestModel.beginDate);
                            DateTime endDate = GetDateTime(requestModel.endDate).AddDays(1).AddSeconds(-1);
                            cmd.Parameters.Add("@startdate", SqlDbType.DateTime).Value = startDate;
                            cmd.Parameters.Add("@enddate", SqlDbType.DateTime).Value = endDate;
                            using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                            {
                                while (await dr.ReadAsync())
                                {
                                    deviceDetailList.Add(new PlaybackDataModel
                                    {
                                        speed = GetInt(dr["speed"]),
                                        datadate = GetDateTime(dr["datadate"]),
                                        acignition = GetString(dr["acignition"]) == "1" ? "Off" : "On",
                                        distance = GetDecimal(dr["distance"]),
                                        location = GetString(dr["loc"])
                                    });
                                }
                            }
                        }
                    }
                    // ================= IGNITION CALCULATION =================
                    double totalIgnitionSeconds = 0;
                    int ignitionOnCount = 0;
                    DateTime? ignitionStart = null;
                    string startLocation = "";
                    foreach (var record in deviceDetailList.OrderBy(x => x.datadate))
                    {
                        if (record.acignition == "On")
                        {
                            if (ignitionStart == null)
                            {
                                ignitionStart = record.datadate;
                                startLocation = record.location;
                                ignitionOnCount++;
                            }
                        }
                        else
                        {
                            if (ignitionStart != null)
                            {
                                TimeSpan duration = record.datadate - ignitionStart.Value;

                                totalIgnitionSeconds += duration.TotalSeconds;

                                item.objIgnitionStatusReport.Add(new IgnitionStatus
                                {
                                    IgnitionOnTime = ignitionStart.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                                    IgnitionOffTime = record.datadate.ToString("yyyy-MM-dd HH:mm:ss"),
                                    Duration = string.Format("{0:D2}-{1:D2}:{2:D2}:{3:D2}",
                                    duration.Days,
                                    duration.Hours,
                                    duration.Minutes,
                                    duration.Seconds),
                                    SLocation = startLocation,
                                    ELocation = record.location
                                });

                                ignitionStart = null;
                            }
                        }
                    }

                    // Ignition still ON at end of data
                    if (ignitionStart != null && deviceDetailList.Count > 0)
                    {
                        DateTime lastTime = deviceDetailList.Last().datadate;

                        TimeSpan duration = lastTime - ignitionStart.Value;

                        totalIgnitionSeconds += duration.TotalSeconds;

                        item.objIgnitionStatusReport.Add(new IgnitionStatus
                        {
                            IgnitionOnTime = ignitionStart.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                            IgnitionOffTime = lastTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            Duration = string.Format(
                                "{0} Day(s)-{1} Hour(s):{2} Minute(s):{3} Second(s)",
                                duration.Days,
                                duration.Hours,
                                duration.Minutes,
                                duration.Seconds),
                            SLocation = startLocation,
                            ELocation = deviceDetailList.Last().location
                        });
                    }

                    item.IgnitionOnOffCounter = ignitionOnCount.ToString();

                    TimeSpan totalDuration = TimeSpan.FromSeconds(totalIgnitionSeconds);

                    item.TotalIgnitionTime = string.Format(
                        "{0} Day(s)-{1} Hour(s):{2} Minute(s):{3} Second(s)",
                        totalDuration.Days,
                        totalDuration.Hours,
                        totalDuration.Minutes,
                        totalDuration.Seconds);
                });

                await Task.WhenAll(tasks);

                model.PageCount = TotalCount;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
            }

            return model;
        }



        public async Task<VehicleStatusResponse> VehicleStatus(DataTableRequestModel model)
        {
            var result = new VehicleStatusResponse();
            result.VehicleData = new List<VehicleStatusDto>();

            using (SqlConnection con = new SqlConnection(_connectionString43))
            using (SqlCommand cmd = new SqlCommand("NewTMVehicleStatus", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@custId", model.CustId);
                cmd.Parameters.AddWithValue("@LowerBand", model.iDisplayStart);
                cmd.Parameters.AddWithValue("@UpperBand", model.iDisplayLength);
                cmd.Parameters.AddWithValue("@searchText", (object)model.sSearch ?? DBNull.Value);

                SqlParameter outParam = new SqlParameter("@ItemCount", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(outParam);

                await con.OpenAsync();

                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                {
                    while (await dr.ReadAsync())
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
                    cmd.Parameters.AddWithValue("@beginDate", model.beginDate);
                    cmd.Parameters.AddWithValue("@EndDate", model.endDate);

                    await con.OpenAsync();

                    using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                    {
                        while (await dr.ReadAsync())
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

        public async Task<BatteryDisconnectionResponse> BatteryDisconnection(DataTableRequestModel model)
        {
            var result = new BatteryDisconnectionResponse();
            result.VehicleData = new List<BatteryDisconnectionDto>(); 

            using (SqlConnection con = new SqlConnection(_connectionString43))
            using (SqlCommand cmd = new SqlCommand("NewTMVehicleStatus", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@custId", model.CustId);
                cmd.Parameters.AddWithValue("@LowerBand", model.iDisplayStart);
                cmd.Parameters.AddWithValue("@UpperBand", model.iDisplayLength);
                cmd.Parameters.AddWithValue("@searchText", (object)model.sSearch ?? DBNull.Value);

                SqlParameter outParam = new SqlParameter("@ItemCount", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(outParam);

                await con.OpenAsync();

                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        result.VehicleData.Add(new BatteryDisconnectionDto
                        {
                            RowNo = GetInt(dr["RowNo"]),
                            BBID = GetString(dr["BBID"]),
                            VehName = GetString(dr["vehname"]),
                            Logs = new List<BatteryDisconnectionLogDto>()
                        });
                    }
                }

                result.ItemCount = Convert.ToInt32(outParam.Value);
            }

            foreach (var item in result.VehicleData)
            {
                using (SqlConnection con = new SqlConnection(_connectionString43))
                using (SqlCommand cmd = new SqlCommand("GetBatteryDisconnectionTM", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@TableName", item.BBID);
                    cmd.Parameters.AddWithValue("@beginDate", model.beginDate);
                    cmd.Parameters.AddWithValue("@EndDate", model.endDate);

                    await con.OpenAsync();

                    using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            item.Logs.Add(new BatteryDisconnectionLogDto
                            {
                                Batterydisc = GetDateTime(dr["startdate"]),
                                Batterycon = GetDateTime(dr["enddate"]),
                                Startloc = GetString(dr["sloc"]),
                                Endloc = GetString(dr["eloc"]),
                                Duration = GetString(dr["duration"]),
                                Status = GetString(dr["Status"])
                            });
                        }
                    }
                }
            }

            return result;
        }

        public async Task<(List<StoppageSubModel> data, int TotalCount)> GetCombinedStoppageReport(
            DataTableRequestModel dtmodel)
        {
            var result = new List<StoppageSubModel>();
            int TotalCount = 0;
            try
            {
                // ================= MAIN DATA =================
                using (SqlConnection con = new SqlConnection(_connectionString43))
                using (SqlCommand cmd = new SqlCommand("GetVehiclesByCustIdAndSearch", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@custId", dtmodel.CustId);
                    cmd.Parameters.AddWithValue("@iDisplayStart", dtmodel.iDisplayStart);
                    cmd.Parameters.AddWithValue("@iDisplayLength", dtmodel.iDisplayLength);
                    cmd.Parameters.AddWithValue("@sortColumn", dtmodel.sortColumn);
                    cmd.Parameters.AddWithValue("@sortDirection", dtmodel.sortDirection);
                    cmd.Parameters.AddWithValue("@sSearch", dtmodel.sSearch);
                    SqlParameter totalCountParam = new SqlParameter("@TotalCount", SqlDbType.Int);
                    totalCountParam.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(totalCountParam);
                    await con.OpenAsync();
                    using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            result.Add(new StoppageSubModel
                            {
                                BBID = GetString(dr["BBID"]),
                                VehicleName = GetString(dr["VehName"]),
                                DriverName = GetString(dr["DriverName"]),
                                objStoppageReport =
                                    new List<StoppageAnalysis>()
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

                        string query = $@"SELECT speed,datadate,acignition,distance,loc FROM  [{item.BBID}] WHERE datadate >= @startdate AND datadate <= @enddate ORDER BY datadate ASC";

                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            DateTime startDate = GetDateTime(dtmodel.beginDate);
                            DateTime endDate = GetDateTime(dtmodel.endDate).AddDays(1).AddSeconds(-1);
                            cmd.Parameters.Add("@startdate", SqlDbType.DateTime).Value = startDate;
                            cmd.Parameters.Add("@enddate", SqlDbType.DateTime).Value = endDate;
                            using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                            {
                                while (await dr.ReadAsync())
                                {
                                    deviceDetailList.Add(new PlaybackDataModel
                                    {
                                        speed = GetInt(dr["speed"]),
                                        datadate = GetDateTime(dr["datadate"]),
                                        // IMPORTANT:
                                        // 1 = OFF
                                        // 0 = ON
                                        acignition = GetString(dr["acignition"]) == "1" ? "Off" : "On",
                                        distance = GetDecimal(dr["distance"]),
                                        location = GetString(dr["loc"])
                                    });
                                }
                            }
                        }
                    }
                    bool flag = false; string interval = dtmodel.Interval ?? "0-0";
                    int intv1 = 0;
                    int intv2 = 0;
                    string[] words = interval.Split('-');
                    if (words.Length > 0)
                        intv1 = Convert.ToInt32(words[0]) * 60;
                    if (words.Length > 1)
                        intv2 = Convert.ToInt32(words[1]) * 60;
                    DateTime startd = DateTime.MinValue;
                    DateTime endd = DateTime.MinValue;
                    TimeSpan totalDuration = TimeSpan.Zero;
                    StoppageAnalysis currentStop = null;
                    int resultIndex = result.FindIndex(x => x.BBID == item.BBID);
                    for (int i = 0; i < deviceDetailList.Count; i++)
                    {
                        var data = deviceDetailList[i];
                        bool ignitionOff = data.acignition == "Off";
                        bool ignitionOn = data.acignition == "On";
                        // =========================================
                        // START STOPPAGE
                        // SAME AS ORIGINAL CODE
                        // =========================================
                        if (ignitionOff && flag == false)
                        {
                            currentStop = new StoppageAnalysis
                            {
                                StopDateAndTime = data.datadate.ToString("yyyy-MM-dd HH:mm:ss"),
                                Location = data.location,
                                IgnitionStatus = false,
                                Duration = "0 minute(s) 0 second(s)"
                            };
                            startd = data.datadate;
                            endd = data.datadate;
                            flag = true;
                        }
                        // =========================================
                        // CONTINUE STOPPAGE
                        // UPDATE END TIME
                        // =========================================
                        else if (ignitionOff && flag == true)
                        {
                            endd = data.datadate;
                        }
                        // =========================================
                        // CLOSE STOPPAGE
                        // ONLY WHEN IGNITION ON
                        // =========================================

                        else if (ignitionOn && flag == true)
                        {
                            // EXACT OLD LOGIC

                            if (endd < data.datadate)
                            {
                                endd = data.datadate;
                            }
                            TimeSpan ts = endd.Subtract(startd);
                            // IMPORTANT:
                            // skip zero duration stoppage
                            if (ts.TotalSeconds > 0)
                            {
                                if (ts.TotalSeconds > 0)
                                {
                                    bool shouldAdd = false;
                                    // 0-0 = old logic (show all)
                                    if (intv1 == 0 && intv2 == 0)
                                    {
                                        shouldAdd = true;
                                    }
                                    else if (intv1 == 0 && intv2 > 0)
                                    {
                                        if (ts.TotalSeconds <= intv2)
                                        {
                                            shouldAdd = true;
                                        }
                                    }

                                    // 10-0 = greater than 10 minute
                                    else if (intv1 > 0 && intv2 == 0)
                                    {
                                        if (ts.TotalSeconds >= intv1)
                                        {
                                            shouldAdd = true;
                                        }
                                    }
                                    // 1-2 = between 1 and 2 minute
                                    else
                                    {
                                        if (ts.TotalSeconds >= intv1 &&
                                            ts.TotalSeconds <= intv2)
                                        {
                                            shouldAdd = true;
                                        }
                                    }
                                    if (shouldAdd)
                                    {
                                        currentStop.Duration = $"{ts.Days:D2}-{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
                                        totalDuration += ts;
                                        result[resultIndex].objStoppageReport.Add(currentStop);
                                        result[resultIndex].StoppageCount++;
                                    }
                                }
                                //currentStop.Duration =
                                //    $"{ts.Days:D2}-{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";

                                //totalDuration += ts;

                                //result[resultIndex]
                                //    .objStoppageReport
                                //    .Add(currentStop);

                                //result[resultIndex].StoppageCount++;
                            }
                            flag = false;
                        }
                    }
                    // =========================================
                    // HANDLE LAST RECORD
                    // =========================================

                    if (flag && currentStop != null)
                    {
                        TimeSpan ts = endd.Subtract(startd);

                        if (ts.TotalSeconds > 0)
                        {
                            if (ts.TotalSeconds > 0)
                            {
                                bool shouldAdd = false;

                                // 0-0 = old logic (show all)
                                if (intv1 == 0 && intv2 == 0)
                                {
                                    shouldAdd = true;
                                }
                                else if (intv1 == 0 && intv2 > 0)
                                {
                                    if (ts.TotalSeconds <= intv2)
                                    {
                                        shouldAdd = true;
                                    }
                                }
                                // 10-0 = greater than 10 minute
                                else if (intv1 > 0 && intv2 == 0)
                                {
                                    if (ts.TotalSeconds >= intv1)
                                    {
                                        shouldAdd = true;
                                    }
                                }

                                // 1-2 = between 1 and 2 minute
                                else
                                {
                                    if (ts.TotalSeconds >= intv1 &&
                                        ts.TotalSeconds <= intv2)
                                    {
                                        shouldAdd = true;
                                    }
                                }

                                if (shouldAdd)
                                {
                                    currentStop.Duration =
                                        $"{ts.Days:D2}-{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";

                                    totalDuration += ts;

                                    result[resultIndex]
                                        .objStoppageReport
                                        .Add(currentStop);

                                    result[resultIndex].StoppageCount++;
                                }
                            }
                            //currentStop.Duration =
                            //    $"{ts.Days:D2}-{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";

                            //totalDuration += ts;

                            //result[resultIndex]
                            //    .objStoppageReport
                            //    .Add(currentStop);

                            //result[resultIndex].StoppageCount++;
                        }
                    }

                    result[resultIndex].TotalStoppageTime =
                        $"{totalDuration.Days} day(s) " +
                        $"{totalDuration.Hours} hour(s) " +
                        $"{totalDuration.Minutes} minute(s) " +
                        $"{totalDuration.Seconds} second(s)";
                });

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            return (result, TotalCount);
        }



        //private bool IsValidIdling(TimeSpan ts, int intv1, int intv2)
        //{
        //    if (intv1 <= 0)
        //        return true;

        //    if (intv1 >= 1500)
        //        return ts.TotalSeconds >= intv1;

        //    return ts.TotalSeconds >= intv1 &&
        //           ts.TotalSeconds <= intv2;
        //}

        private bool IsValidIdling(TimeSpan ts, int intv1, int intv2)
        {
            double durationSeconds = ts.TotalSeconds;

            // All
            if (intv1 == 0 && intv2 == 0)
                return true;

            int minSeconds = intv1 * 60;
            int maxSeconds = intv2 * 60;

            return durationSeconds >= minSeconds &&
                   durationSeconds < maxSeconds;
        }

        public async Task<(List<IdlingMainModel> data, int TotalCount)> GetIdlingStatusReport(DataTableRequestModel dtmodel)
        {
            var result = new List<IdlingMainModel>();
            int TotalCount = 0;

            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString43))
                using (SqlCommand cmd =
                       new SqlCommand("GetVehiclesByCustIdAndSearch", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@custId", dtmodel.CustId);
                    cmd.Parameters.AddWithValue("@iDisplayStart", dtmodel.iDisplayStart);
                    cmd.Parameters.AddWithValue("@iDisplayLength", dtmodel.iDisplayLength);
                    cmd.Parameters.AddWithValue("@sortColumn", dtmodel.sortColumn);
                    cmd.Parameters.AddWithValue("@sortDirection", dtmodel.sortDirection);
                    cmd.Parameters.AddWithValue("@sSearch", dtmodel.sSearch);

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
                            result.Add(new IdlingMainModel
                            {
                                BBID = GetString(dr["BBID"]),
                                VehicleName = GetString(dr["VehName"]),
                                DriverName = GetString(dr["DriverName"]),
                                IdlingSubStatuslist = new List<IdlingSubStatus>()
                            });
                        }
                    }

                    TotalCount = Convert.ToInt32(totalCountParam.Value);
                }

                var tasks = result.Select(async item =>
                {
                    var deviceDetailList = new List<PlaybackDataModel>();

                    using (SqlConnection con =
                           new SqlConnection(GetConnectionStringTableWise(item.BBID)))
                    {
                        await con.OpenAsync();

                        //  string query = $@"SELECT speed, datadate,acignition,distance,loc FROM [{item.BBID}] WHERE datadate >= @startdate AND datadate <= @enddate ORDER BY datadate ASC";
                        string query = $@"SELECT speed,datadate,acignition,distance,loc,latitude,longitude FROM [{item.BBID}] WHERE datadate >= @startdate AND datadate <= @enddate ORDER BY datadate ASC";

                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            DateTime startDate =
                                GetDateTime(dtmodel.beginDate);

                            DateTime endDate =
                                GetDateTime(dtmodel.endDate)
                                .AddDays(1)
                                .AddSeconds(-1);

                            cmd.Parameters.Add("@startdate",
                                SqlDbType.DateTime).Value = startDate;

                            cmd.Parameters.Add("@enddate",
                                SqlDbType.DateTime).Value = endDate;

                            using (SqlDataReader dr =
                                   await cmd.ExecuteReaderAsync())
                            {
                                while (await dr.ReadAsync())
                                {
                                    deviceDetailList.Add(new PlaybackDataModel
                                    {
                                        speed = GetInt(dr["speed"]),
                                        datadate = GetDateTime(dr["datadate"]),
                                        acignition = GetString(dr["acignition"]),
                                        distance = GetDecimal(dr["distance"]),
                                        location = GetString(dr["loc"]),
                                        latitude = GetDecimal(dr["latitude"]),
                                        longitude = GetDecimal(dr["longitude"])
                                    });
                                }
                            }
                        }
                    }

                    string interval = dtmodel.Interval ?? "0-0";

                    int intv1 = 0;
                    int intv2 = 0;

                    string[] words = interval.Split('-');

                    if (words.Length > 0)
                        intv1 = Convert.ToInt32(words[0]);

                    if (words.Length > 1)
                        intv2 = Convert.ToInt32(words[1]);

                    bool flag = false;

                    DateTime startd = DateTime.MinValue;
                    DateTime endd = DateTime.MinValue;

                    TimeSpan totalDuration = TimeSpan.Zero;

                    IdlingSubStatus currentStop = null;

                    int resultIndex = result.FindIndex(x => x.BBID == item.BBID);

                    foreach (var data in deviceDetailList)
                    {
                        int ignition;

                        if (!int.TryParse(data.acignition, out ignition))
                        {
                            ignition = data.acignition.Equals("Off",
                                StringComparison.OrdinalIgnoreCase)
                                ? 1
                                : 0;
                        }

                        // START IDLING
                        if (ignition == 0 &&
                            data.speed == 0 &&
                            !flag)
                        {
                            currentStop = new IdlingSubStatus
                            {
                                startDate = data.datadate.ToString("yyyy-MM-dd HH:mm:ss"),
                                location = data.location,
                                latitude = data.latitude,
                                longitude = data.longitude,
                                IgnitionStatus = false
                            };

                            startd = data.datadate;
                            endd = data.datadate;
                            flag = true;
                        }

                        // CONTINUE IDLING
                        else if (ignition == 0 &&
                                 data.speed == 0 &&
                                 flag)
                        {
                            endd = data.datadate;

                            currentStop.location = data.location;
                            currentStop.latitude = data.latitude;
                            currentStop.longitude = data.longitude;
                        }

                        // IGNITION OFF
                        else if (ignition == 1 &&
                                 data.speed == 0 &&
                                 flag)
                        {
                            endd = data.datadate;

                            TimeSpan ts = endd.Subtract(startd);

                            if (ts.TotalSeconds > 0 &&
                                IsValidIdling(ts, intv1, intv2))
                            {
                                currentStop.stopDate =
                                    endd.ToString("yyyy-MM-dd HH:mm:ss");

                                currentStop.location = data.location;
                                currentStop.latitude = data.latitude;
                                currentStop.longitude = data.longitude;

                                currentStop.Vstatus = "Ignition switch Off";

                                currentStop.duration =
                                    $"{ts.Days:D2}-{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";

                                totalDuration = totalDuration.Add(ts);

                                result[resultIndex]
                                    .IdlingSubStatuslist
                                    .Add(currentStop);
                            }

                            flag = false;
                            currentStop = null;
                        }

                        // VEHICLE MOVED
                        else if (ignition == 0 &&
                                 data.speed > 0 &&
                                 flag)
                        {
                            if (endd < data.datadate)
                            {
                                endd = data.datadate;
                            }

                            TimeSpan ts = endd.Subtract(startd);

                            if (ts.TotalSeconds > 0 &&
                                IsValidIdling(ts, intv1, intv2))
                            {
                                currentStop.stopDate =
                                    endd.ToString("yyyy-MM-dd HH:mm:ss");

                                currentStop.location = data.location;
                                currentStop.latitude = data.latitude;
                                currentStop.longitude = data.longitude;

                                currentStop.Vstatus = "Vehicle Moved";

                                currentStop.duration =
                                    $"{ts.Days:D2}-{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";

                                totalDuration = totalDuration.Add(ts);

                                result[resultIndex]
                                    .IdlingSubStatuslist
                                    .Add(currentStop);
                            }

                            flag = false;
                            currentStop = null;
                        }
                    }

                    // Handle open idling session at end
                    if (flag && currentStop != null)
                    {
                        TimeSpan ts = endd.Subtract(startd);

                        if (ts.TotalSeconds > 0 &&
                            IsValidIdling(ts, intv1, intv2))
                        {
                            currentStop.stopDate =
                                endd.ToString("yyyy-MM-dd HH:mm:ss");

                            currentStop.Vstatus = "Open Session";

                            currentStop.duration =
                                $"{ts.Days:D2}-{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";

                            totalDuration = totalDuration.Add(ts);

                            result[resultIndex]
                                .IdlingSubStatuslist
                                .Add(currentStop);
                        }
                    }

                    result[resultIndex].idlingCount =
                        result[resultIndex].IdlingSubStatuslist.Count;

                    result[resultIndex].TotalIdlingTime =
                        $"{totalDuration.Days} day(s) " +
                        $"{totalDuration.Hours} hour(s) " +
                        $"{totalDuration.Minutes} minute(s)";
                });

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return (result, TotalCount);
        }


        public async Task<(List<DistanceReportDataModel> data, int TotalCount)> GetDistanceReportData(DataTableRequestModel model)
        {
            var result = new List<DistanceReportDataModel>();
            int TotalCount = 0;
            try
            {

                // ================= MAIN DATA =================

                using (SqlConnection con = new SqlConnection(_connectionString43))
                using (SqlCommand cmd = new SqlCommand("GetVehiclesByCustIdAndSearch", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@custId", model.CustId);
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
                            cmd.Parameters.AddWithValue("@startdate", GetDateTime(model.beginDate));
                            cmd.Parameters.AddWithValue("@enddate", GetDateTime(model.endDate));

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
                                        StartTime = tripStartTime.Value.ToString("MMM dd yyyy hh:mm tt"),

                                        EndTime = tripEndTime.ToString("MMM dd yyyy hh:mm tt"),

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
                                    StartTime = tripStartTime.Value.ToString("MMM dd yyyy hh:mm tt"),

                                    EndTime = tripEndTime.ToString("MMM dd yyyy hh:mm tt"),

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
            return (result, TotalCount);
        }
        public async Task<(List<DistanceMonthlyReportDataModel> data, int TotalCount)> GetMonthlyDistanceReportData(DataTableRequestModel model)
        {
            var result = new List<DistanceMonthlyReportDataModel>();

            int TotalCount = 0;

            try
            {
                DateTime monthDate =
                    DateTime.ParseExact(
                        model.beginDate,
                        "MMMM yyyy",
                        CultureInfo.InvariantCulture);

                DateTime startDate =
                    new DateTime(
                        monthDate.Year,
                        monthDate.Month,
                        1);

                DateTime endDate =
                    startDate
                    .AddMonths(1)
                    .AddSeconds(-1);

                int daysInMonth =
                    DateTime.DaysInMonth(
                        monthDate.Year,
                        monthDate.Month);

                // ================= MAIN VEHICLE DATA =================

                using (SqlConnection con =
                       new SqlConnection(_connectionString43))

                using (SqlCommand cmd =
                       new SqlCommand(
                           "GetVehiclesByCustIdAndSearch",
                           con))
                {
                    cmd.CommandType =
                        CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue(
                        "@custId",
                        model.CustId);

                    cmd.Parameters.AddWithValue(
                        "@iDisplayStart",
                        model.iDisplayStart);

                    cmd.Parameters.AddWithValue(
                        "@iDisplayLength",
                        model.iDisplayLength);

                    cmd.Parameters.AddWithValue(
                        "@sortColumn",
                        model.sortColumn);

                    cmd.Parameters.AddWithValue(
                        "@sortDirection",
                        model.sortDirection);

                    cmd.Parameters.AddWithValue(
                        "@sSearch",
                        model.sSearch);

                    SqlParameter totalCountParam =
                        new SqlParameter(
                            "@TotalCount",
                            SqlDbType.Int);

                    totalCountParam.Direction =
                        ParameterDirection.Output;

                    cmd.Parameters.Add(totalCountParam);

                    await con.OpenAsync();

                    using (SqlDataReader dr =
                           await cmd.ExecuteReaderAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            result.Add(
                                new DistanceMonthlyReportDataModel
                                {
                                    BBID =
                                        GetString(dr["BBID"]),

                                    VehName =
                                        GetString(dr["VehName"]),

                                    TotalDistance = "0.0",

                                    TotalStoppage = "0.0",

                                    _distanceMonthlyReportSubDataModels =
                                        new List<DistanceMonthlyReportSubDataModel>()
                                });
                        }
                    }

                    TotalCount =
                        Convert.ToInt32(totalCountParam.Value);
                }

                // ================= VEHICLE PARALLEL =================

                var vehicleTasks =
                    result.Select(async item =>
                    {
                        var deviceDetailList =
                            new List<PlaybackDataModel>();

                        using (SqlConnection con =
                               new SqlConnection(
                                   GetConnectionStringTableWise(item.BBID)))
                        {
                            await con.OpenAsync();

                            string query = $@"
SELECT speed,
       datadate,
       acignition,
       distance,
       loc
FROM [{item.BBID}]
WHERE datadate >= @startdate
AND datadate <= @enddate
ORDER BY datadate";

                            using (SqlCommand cmd =
                                   new SqlCommand(query, con))
                            {
                                cmd.Parameters.AddWithValue(
                                    "@startdate",
                                    startDate);

                                cmd.Parameters.AddWithValue(
                                    "@enddate",
                                    endDate);

                                using (SqlDataReader dr =
                                       await cmd.ExecuteReaderAsync())
                                {
                                    while (await dr.ReadAsync())
                                    {
                                        deviceDetailList.Add(
                                            new PlaybackDataModel
                                            {
                                                speed =
                                                    GetInt(dr["speed"]),

                                                datadate =
                                                    GetDateTime(dr["datadate"]),

                                                acignition =
                                                    GetString(dr["acignition"]) == "1"
                                                    ? "Off"
                                                    : "On",

                                                distance =
                                                    GetDecimal(dr["distance"]),

                                                location =
                                                    GetString(dr["loc"])
                                            });
                                    }
                                }
                            }
                        }

                        // ================= DAY-WISE =================

                        var dayTasks =
                            Enumerable.Range(1, daysInMonth)
                            .Select(async day =>
                            {
                                return await Task.Run(() =>
                                {
                                    DateTime currentDay =
                                        new DateTime(
                                            monthDate.Year,
                                            monthDate.Month,
                                            day);

                                    var dayRecords =
                                        deviceDetailList
                                        .Where(x =>
                                            x.datadate.Date ==
                                            currentDay.Date)
                                        .OrderBy(x => x.datadate)
                                        .ToList();

                                    // NO DATA
                                    if (!dayRecords.Any())
                                    {
                                        return new DistanceMonthlyReportSubDataModel
                                        {
                                            Day = day,
                                            Distance = "0.0",
                                            Duration = "0.0"
                                        };
                                    }

                                    // ================= DISTANCE LOGIC =================

                                    bool moveFlag = false;

                                    decimal sdist = 0;
                                    decimal edist = 0;

                                    decimal totalDistance = 0;

                                    for (int i = 0; i < dayRecords.Count; i++)
                                    {
                                        var current =
                                            dayRecords[i];

                                        decimal speed =
                                            current.speed;

                                        // START
                                        if (speed > 0 && !moveFlag)
                                        {
                                            sdist =
                                                (i == 0)
                                                ? current.distance
                                                : dayRecords[i - 1].distance;

                                            edist =
                                                current.distance;

                                            moveFlag = true;
                                        }

                                        // CONTINUE
                                        else if (speed > 0 && moveFlag)
                                        {
                                            edist =
                                                current.distance;
                                        }

                                        // STOP
                                        else if (speed <= 0 && moveFlag)
                                        {
                                            edist =
                                                current.distance;

                                            decimal tripDistance =
                                                Math.Round(
                                                    edist - sdist,
                                                    1);

                                            if (tripDistance > 0 &&
                                                tripDistance < 500)
                                            {
                                                totalDistance +=
                                                    tripDistance;
                                            }

                                            moveFlag = false;
                                        }
                                    }

                                    // LAST RUNNING SESSION
                                    if (moveFlag)
                                    {
                                        decimal tripDistance =
                                            Math.Round(
                                                edist - sdist,
                                                1);

                                        if (tripDistance > 0 &&
                                            tripDistance < 500)
                                        {
                                            totalDistance +=
                                                tripDistance;
                                        }
                                    }

                                    // ================= STOPPAGE LOGIC =================

                                    bool stopFlag = false;

                                    DateTime stopStart =
                                        DateTime.MinValue;

                                    DateTime stopEnd =
                                        DateTime.MinValue;

                                    TimeSpan totalStoppage =
                                        TimeSpan.Zero;

                                    for (int i = 0; i < dayRecords.Count; i++)
                                    {
                                        var data =
                                            dayRecords[i];

                                        bool ignitionOff =
                                            data.acignition == "Off";

                                        bool ignitionOn =
                                            data.acignition == "On";

                                        // START STOPPAGE
                                        if (ignitionOff && !stopFlag)
                                        {
                                            stopStart =
                                                data.datadate;

                                            stopEnd =
                                                data.datadate;

                                            stopFlag = true;
                                        }

                                        // CONTINUE STOPPAGE
                                        else if (ignitionOff && stopFlag)
                                        {
                                            stopEnd =
                                                data.datadate;
                                        }

                                        // END STOPPAGE
                                        else if (ignitionOn && stopFlag)
                                        {
                                            if (stopEnd < data.datadate)
                                            {
                                                stopEnd =
                                                    data.datadate;
                                            }

                                            TimeSpan ts =
                                                stopEnd.Subtract(stopStart);

                                            if (ts.TotalSeconds > 0)
                                            {
                                                totalStoppage += ts;
                                            }

                                            stopFlag = false;
                                        }
                                    }

                                    // HANDLE LAST STOPPAGE
                                    if (stopFlag)
                                    {
                                        TimeSpan ts =
                                            stopEnd.Subtract(stopStart);

                                        if (ts.TotalSeconds > 0)
                                        {
                                            totalStoppage += ts;
                                        }
                                    }

                                    return new DistanceMonthlyReportSubDataModel
                                    {
                                        Day = day,

                                        Distance =
                                            totalDistance.ToString("0.0"),

                                        Duration =
                                            totalStoppage.TotalHours.ToString("0.0")
                                    };
                                });
                            });

                        var dayResults =
                            await Task.WhenAll(dayTasks);

                        item._distanceMonthlyReportSubDataModels =
                            dayResults
                            .OrderBy(x => x.Day)
                            .ToList();

                        // ================= TOTAL DISTANCE =================

                        decimal totalVehicleDistance =
                            dayResults.Sum(x =>
                                decimal.TryParse(
                                    x.Distance,
                                    out decimal d)
                                ? d
                                : 0);

                        item.TotalDistance =
                            totalVehicleDistance.ToString("0.0");

                        // ================= TOTAL STOPPAGE =================

                        double totalVehicleStoppage =
                            dayResults.Sum(x =>
                                double.TryParse(
                                    x.Duration,
                                    out double d)
                                ? d
                                : 0);

                        item.TotalStoppage =
                            totalVehicleStoppage.ToString("0.0");

                        item.TotalStoppage =
                            totalVehicleStoppage.ToString("0.0");
                    });

                // ================= ALL VEHICLES =================

                await Task.WhenAll(vehicleTasks);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return (result, TotalCount);
        }


        #region Neha Vaid  
        public async Task<OverSpeedModel> getSpeedReport(string mode, DataTableRequestModel requestModel)
        {
            var model = new OverSpeedModel();
            model.OSmainLst = new List<overSpeedMain>();

            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString43))
                using (SqlCommand cmd = new SqlCommand("NewTMVehicleStatus", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@LowerBand", requestModel.iDisplayStart);
                    cmd.Parameters.AddWithValue("@UpperBand", requestModel.iDisplayLength);
                    cmd.Parameters.AddWithValue("@custId", requestModel.CustId);
                    cmd.Parameters.AddWithValue("@searchText", requestModel.sSearch);

                    SqlParameter outParam = new SqlParameter("@ItemCount", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(outParam);
                    await con.OpenAsync();

                    using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            model.OSmainLst.Add(new overSpeedMain
                            {
                                bbid = GetString(dr["BBID"]),
                                vehName = GetString(dr["VehName"]),
                                driverName = GetString(dr["DriverName"]),
                                overSpeedVal = GetInt(dr["overspeed"]),
                                OSsublst = new List<OverSpeedAnalysis>()
                            });

                        }

                    }
                    model.PageCount = Convert.ToInt32(outParam.Value);
                }
                foreach (var item in model.OSmainLst)
                {
                    var speedSublist = new List<OverSpeedAnalysis>();
                    using (SqlConnection con = new SqlConnection(GetConnectionStringTableWise(item.bbid)))
                    {
                        await con.OpenAsync();

                        string maxSpeedQuery = @"SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED; SELECT MAX(Speed) FROM " + item.bbid + @" WHERE datadate >= @BeginDate AND datadate <= @EndDate AND acignition = 0";
                        using (SqlCommand cmd = new SqlCommand(maxSpeedQuery, con))
                        {
                            cmd.Parameters.AddWithValue("@BeginDate", requestModel.beginDate);
                            cmd.Parameters.AddWithValue("@EndDate", requestModel.endDate);

                            var result = await cmd.ExecuteScalarAsync();
                            item.maxSpeed = result != DBNull.Value && result != null ? Convert.ToInt32(result) : 0;
                        }
                        var dyn = 3;
                        if (mode == "over")// this condition depends upon report type i.e overspeed or speed analysis.
                        {
                            dyn = 1;
                        }
                        int OverCount = 0;
                        int totalSpeed = 0;
                        DateTime previousDateTime = DateTime.MinValue;
                        TimeSpan overspeedDuration = new TimeSpan(0, 0, 0);
                        SqlParameter[] parameters =
                        {
                            new SqlParameter("@beginDate", requestModel.beginDate),
                            new SqlParameter("@endDate", requestModel.endDate),
                            new SqlParameter("@bBid", item.bbid),
                            new SqlParameter("@mode", "over")
                        };

                        DataSet ds = SqlHelper.ExecuteDataset(con, CommandType.StoredProcedure, "SpeedAnalysisTM", parameters);
                        if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                        {
                            DataTable dt = ds.Tables[0];

                            for (int i = 0; i < dt.Rows.Count; i = i + dyn)
                            {
                                DataRow row = dt.Rows[i];

                                OverSpeedAnalysis sublistObj = new OverSpeedAnalysis();
                                sublistObj.speed = GetInt(row["Speed"]);
                                sublistObj.dateTime = GetDateTime(row["datadate"]);
                                sublistObj.location = GetString(row["Location"]);
                                sublistObj.latitude = GetFloat(row["latitude"]);
                                sublistObj.longitude = GetFloat(row["longitude"]);
                                totalSpeed = totalSpeed + GetInt(row["Speed"]);


                                speedSublist.Add(sublistObj);

                                if (requestModel.CustId != 6387)
                                    OverCount++;
                                else if (sublistObj.speed >= item.overSpeedVal)
                                    OverCount++;

                                if (!previousDateTime.Equals(DateTime.MinValue))
                                {
                                    TimeSpan ts = sublistObj.dateTime.Subtract(previousDateTime);

                                    if (ts.Days == 0 && ts.Hours == 0 && ts.Minutes < 1)
                                    {
                                        overspeedDuration = overspeedDuration.Add(ts);
                                    }
                                }

                                previousDateTime = sublistObj.dateTime;
                            }
                            item.totalSpeed = GetInt(totalSpeed);
                            item.overspeedCount = OverCount;

                            item.overSpeedDuration =
                                overspeedDuration.Hours + " Hour(s) " +
                                overspeedDuration.Minutes + " Minute(s) " +
                                overspeedDuration.Seconds + " Second(s)";
                        }
                        else
                        {
                            item.overSpeedDuration = "0 Hour(s) 0 Minute(s) 0 Second(s)";
                        }

                        item.OSsublst = speedSublist;

                    }

                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            return model;
        }
        #endregion


        public async Task<EntryExitReport> GetListofEntryExit( DataTableRequestModel requestModel,string rtype,string bbid)
        {
            EntryExitReport modelObj = new EntryExitReport();
            modelObj.vehicleList = new List<POIEntryExitModelExt>();
            string type = null;
            try
            {
                DataTable dataT = new DataTable();

                using (SqlConnection con = new SqlConnection(_connectionString43))
                using (SqlCommand cmd = new SqlCommand("New_getpoidetailsforbbid", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@LowerBand", requestModel.iDisplayStart);
                    cmd.Parameters.AddWithValue("@UpperBand", requestModel.iDisplayStart + requestModel.iDisplayLength);
                    cmd.Parameters.AddWithValue("@sortColumn", requestModel.sortColumn);
                    cmd.Parameters.AddWithValue("@sortDirection", requestModel.sortDirection);

                    cmd.Parameters.Add("@ItemCount", SqlDbType.Int).Direction =ParameterDirection.Output;

                    cmd.Parameters.AddWithValue("@custId", requestModel.CustId);

                    cmd.Parameters.AddWithValue("@BBID",string.IsNullOrWhiteSpace(bbid)? (object)DBNull.Value: bbid);

                    cmd.Parameters.Add("@searchText", SqlDbType.VarChar).Value =string.IsNullOrWhiteSpace(requestModel.sSearch)? DBNull.Value : requestModel.sSearch;

                    cmd.Parameters.AddWithValue("@Type",string.IsNullOrWhiteSpace(type)? (object)DBNull.Value: type);

                    cmd.Parameters.AddWithValue("@From", requestModel.beginDate);
                    cmd.Parameters.AddWithValue("@To", requestModel.endDate);

                    await con.OpenAsync();

                    using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                    {
                        dataT.Load(dr);
                    }

                    modelObj.PageCount =GetInt(cmd.Parameters["@ItemCount"].Value);
                }

                var tasks = dataT.AsEnumerable()
                    .Select(async row =>
                    {
                        POIEntryExitModelExt obj = new POIEntryExitModelExt();

                        obj.Bbid = GetString(row["bbid"]);
                        obj.VehName = GetString(row["VehName"]);
                        obj.driverName = GetString(row["DriverName"]);
                        obj.poisCoveredList = new List<POIEntryExitModel>();

                        string reportType = (rtype ?? "").Trim();

                        // ====================================================
                        // ENTRY EXIT REPORT
                        // ====================================================
                        if (string.Equals(
                            reportType,
                            "EntryExitReport",
                            StringComparison.OrdinalIgnoreCase))
                        {
                            using (SqlConnection con =
                                new SqlConnection(_defaultConnectionOrange44))
                            using (SqlCommand cmd =
                                new SqlCommand("[dbo].[New_TM_GetPOIDetailsEntryExiy]", con))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;

                                cmd.Parameters.AddWithValue("@From",requestModel.beginDate);

                                cmd.Parameters.AddWithValue("@To",requestModel.endDate);

                                cmd.Parameters.AddWithValue("@Custid", requestModel.CustId);

                                cmd.Parameters.AddWithValue("@BBid",obj.Bbid);

                                await con.OpenAsync();

                                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                                {
                                    DataTable dt = new DataTable();
                                    dt.Load(dr);

                                    //obj.poisCovered = dt.Rows.Count;

                                    for (int i = 0; i < dt.Rows.Count; i++)
                                    {
                                        POIEntryExitModel item = AddData( requestModel.CustId, dt, i,obj.VehName,GetInt( requestModel.Interval));

                                        if (item != null)
                                        {
                                            obj.poisCoveredList.Add(item);
                                        }
                                    }
                                }
                            }
                        }
                        // ====================================================
                        // EXIT ENTRY REPORT
                        // ====================================================
                        else if (string.Equals(reportType, "ExitEntryReport",StringComparison.OrdinalIgnoreCase))
                        {
                            using (SqlConnection con =new SqlConnection(_defaultConnectionOrange44))
                            {
                                await con.OpenAsync();

                                using (SqlCommand cmd =new SqlCommand("NewGetPOIsCoveredExitEntryCustom", con))
                                {
                                    cmd.CommandType = CommandType.StoredProcedure;

                                    cmd.Parameters.AddWithValue("@From",requestModel.beginDate);

                                    cmd.Parameters.AddWithValue("@To",requestModel.endDate);

                                    cmd.Parameters.AddWithValue("@Custid",requestModel.CustId);

                                    cmd.Parameters.AddWithValue("@BBid",obj.Bbid);

                                    DataTable poiTable = new DataTable();

                                    using (SqlDataReader dr =await cmd.ExecuteReaderAsync())
                                    {
                                        poiTable.Load(dr);
                                    }

                                    obj.Totalpois = poiTable.Rows.Count;

                                    foreach (DataRow poiRow in poiTable.Rows)
                                    {
                                        using (SqlCommand detailCmd =new SqlCommand("[dbo].[NewGetPOIDetailsExitEntryCustomy]", con))
                                        {
                                            detailCmd.CommandType =CommandType.StoredProcedure;

                                            detailCmd.Parameters.AddWithValue("@From",requestModel.beginDate);

                                            detailCmd.Parameters.AddWithValue("@To",requestModel.endDate);

                                            detailCmd.Parameters.AddWithValue("@Custid",requestModel.CustId);

                                            detailCmd.Parameters.AddWithValue("@BBid",obj.Bbid);

                                            detailCmd.Parameters.AddWithValue("@poiid", GetInt( poiRow["POIId"]));

                                            DataTable detailTable =new DataTable();

                                            using (SqlDataReader dr =await detailCmd.ExecuteReaderAsync())
                                            {
                                                detailTable.Load(dr);
                                            }

                                            foreach (DataRow detailRow in detailTable.Rows)
                                            {
                                                obj.poisCoveredList.Add(
                                                    new POIEntryExitModel
                                                    {
                                                        POIName = GetString(detailRow["POIName"]),
                                                        duration = GetString(detailRow["duration"]),
                                                        Intime =GetString( detailRow["intime"]),
                                                        OutTime =GetString(detailRow["outtime"])
                                                    });
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        obj.poisCoveredList = obj.poisCoveredList;
                        obj.poisCovered = obj.poisCoveredList.Count();
                        return obj;
                    });

                modelObj.vehicleList =
                    (await Task.WhenAll(tasks)).ToList();
            }
            catch (Exception ex)
            {
                modelObj.vehicleList = new List<POIEntryExitModelExt>();
            }

            return modelObj;
        }
        private POIEntryExitModel AddData(int custid, DataTable dt, int i, string vehName, int seconds)
        {
            POIEntryExitModel poiEntryExitModelObj = new POIEntryExitModel();
            poiEntryExitModelObj.POIID = GetInt(dt.Rows[i]["POIId"]);
            poiEntryExitModelObj.POIName = GetString(dt.Rows[i]["POIName"]);
            poiEntryExitModelObj.Bbid = GetString(dt.Rows[i]["BBID"]);
            string status = "~/resources/images/legends/stop.png";

            string statusid = string.Empty;
            string stptime = "";

            string statusquery = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED select [id],[lat],[longi],[custid],[RetailerId],[Bbid],[details],[StandardDistance],[POIUpdate],[AddedOn],[State],[IsAssign],[AdminId],[IsActive],[StoppageTime],[Direction],[POIStatus] FROM HT_CUST_LATLONG WHERE ID='" + poiEntryExitModelObj.POIID + "' ";

            using (SqlConnection con = new SqlConnection(_connectionString43))
            using (SqlCommand cmd = new SqlCommand(statusquery, con))
            {
                cmd.CommandType = CommandType.Text;

                con.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        statusid = GetString(dr["POIStatus"]);
                        poiEntryExitModelObj.POILat = GetDouble(dr["lat"]);
                        poiEntryExitModelObj.POILong = GetDouble(dr["longi"]);
                    }
                }
            }
            poiEntryExitModelObj.POIName = "<a href='javascript:' onclick=showMapWindow('" + poiEntryExitModelObj.Bbid.Replace(" ", "&nbsp;") + "','" + vehName.Replace(" ", "&nbsp;") + "','" + poiEntryExitModelObj.POILat.ToString().Replace(" ", "&nbsp;") + "','" + poiEntryExitModelObj.POILong.ToString().Replace(" ", "&nbsp;") + "','" + poiEntryExitModelObj.POIName.Replace(" ", "&nbsp;") + "','" + status + "');>" + poiEntryExitModelObj.POIName + "</a>";

            if (custid == 6627)
            {
                if (statusid == "True")

                    poiEntryExitModelObj.IsActive = "<font color='green'>Active</font>";
                else
                    poiEntryExitModelObj.IsActive = "<font color='red'>Inactive</font>";
            }

            poiEntryExitModelObj.Intime = GetString(dt.Rows[i]["InTime"]);

            string bbidnew = GetString(dt.Rows[i]["BBID"]);
            var responseStartDate = StartStopTemp(bbidnew, GetString(poiEntryExitModelObj.Intime));
            if (responseStartDate == "-100")
            {
                responseStartDate = "99";
            }
            poiEntryExitModelObj.StartTempTime = responseStartDate == "99" ? "0" : responseStartDate;

            string query1 = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED select VehName from ht_Main where BBID='" + bbidnew + "' ";
            using (SqlConnection con = new SqlConnection(_connectionString43))
            using (SqlCommand cmd = new SqlCommand(query1, con))
            {
                cmd.CommandType = CommandType.Text;

                con.Open();

                poiEntryExitModelObj.Vehname = GetString(cmd.ExecuteScalar());
            }
            poiEntryExitModelObj.OutTime = Convert.IsDBNull(dt.Rows[i]["OutTime"]) ? string.Empty : GetString(dt.Rows[i]["OutTime"]);
            var responseEndDate = StartStopTemp(bbidnew, GetString(poiEntryExitModelObj.OutTime));
            if (responseEndDate == "-100")
            {
                responseEndDate = "99";
            }
            poiEntryExitModelObj.EndtTempTime = responseEndDate == "99" ? "0" : responseEndDate;
            string Stoppagetime = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED select stoppagetime from ht_cust_latlong whERE  id='" + poiEntryExitModelObj.POIID + "' ";
            using (SqlConnection con = new SqlConnection(_connectionString43))
            using (SqlCommand cmd = new SqlCommand(Stoppagetime, con))
            {
                cmd.CommandType = CommandType.Text;

                con.Open();

                stptime = GetString(cmd.ExecuteScalar());
            }
            var d = GetInt(dt.Rows[i]["Duration"]);
            var timecheck = d / 60;
            var ts = new TimeSpan(0, 0, d);
            if (GetInt(d) > 180)
            {
                poiEntryExitModelObj.duration = "<font color='red'>" + string.Format("{0} Day(s) {1} Hour(s) {2} Minute(s) {3} Second(s)", ts.Days, ts.Hours, ts.Minutes, ts.Seconds) + "</font>";
            }
            else
            {
                poiEntryExitModelObj.duration = string.Format("{0} Day(s) {1} Hour(s) {2} Minute(s) {3} Second(s)", ts.Days, ts.Hours, ts.Minutes, ts.Seconds);
            }
            if (d >= seconds)
            {
                return poiEntryExitModelObj;
            }

            else
            {
                return null;
            }
        }
        private string StartStopTemp(string vehId, string date)
        {
            string responseTemp = "";
            if (!string.IsNullOrEmpty(date))
            {
                var startDate = GetDateTime(date);
                var strcmd4 = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED; select temp2 from " + vehId + " where vehbatvoltage > 8 and datadate ='" + startDate + "'  order by datadate desc";
                using (SqlConnection con = new SqlConnection(GetConnectionStringTableWise(vehId)))
                using (SqlCommand cmd = new SqlCommand(strcmd4, con))
                {
                    cmd.CommandType = CommandType.Text;

                    con.Open();

                    responseTemp = GetString(cmd.ExecuteScalar());
                }
            }
            return responseTemp;
        }
    }
}
