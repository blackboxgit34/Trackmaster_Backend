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
using Trackmaster_Repository.Interface;
using static Trackmaster_Repository.DataTypeHelper;
using static Trackmaster_Model.Reports; //added model

namespace Trackmaster_Repository.Repository
{
    public class ReportsRepository : IReportsRepository

    {
        private readonly string _connectionString43;
        private readonly string _FMSConString43;
        public ReportsRepository(IConfiguration configuration)
        {
            _connectionString43 = configuration.GetConnectionString("DefaultConnection43");
            _FMSConString43 = configuration.GetConnectionString("FMSConString43");
        }

        public VehiclesReport GetConductorInfo(int CustId, int sEcho, int iDisplayStart, int iDisplayLength, string sSearch, string sortColumn, string sortDirection)
        {
            var modelObj = new VehiclesReport();
            modelObj.modelObjList = new List<VehicleInformation>();
            if (sSearch == "null" || string.IsNullOrEmpty(sSearch))
            {
                sSearch = null;
            }
            if (string.IsNullOrEmpty(sortColumn))
                sortColumn = "VehName";
            if (string.IsNullOrEmpty(sortDirection))
                sortDirection = "ASC";
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString43))
                {
                    using (SqlCommand cmd = new SqlCommand("[dbo].[GetCrewData]", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@CustId", CustId);
                        cmd.Parameters.AddWithValue("@startRowIndex", iDisplayStart);
                        cmd.Parameters.AddWithValue("@pageSize", iDisplayLength);
                        cmd.Parameters.AddWithValue("@vehName", string.IsNullOrEmpty(sSearch) ? (object)DBNull.Value : sSearch);
                        cmd.Parameters.AddWithValue("@sortColumn", sortColumn);
                        cmd.Parameters.AddWithValue("@sortDirection", sortDirection);
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


        public string AddUpdateEmployee(Employee objEmp)
        {
            string result = "";

            try
            {
                using (SqlConnection con = new SqlConnection(_FMSConString43))
                using (SqlCommand cmd = new SqlCommand("[dbo].[EmpInfoAddUpdate]", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@EmployeeID", objEmp.EmployeeID);
                    cmd.Parameters.AddWithValue("@Custid", objEmp.Custid);
                    cmd.Parameters.AddWithValue("@EmployeeType", objEmp.EmployeeType);
                    cmd.Parameters.AddWithValue("@contractDuration", objEmp.contractDuration);
                    cmd.Parameters.AddWithValue("@FirstName", objEmp.FirstName);
                    cmd.Parameters.AddWithValue("@LastName", objEmp.LastName);
                    cmd.Parameters.AddWithValue("@HireDate", objEmp.HireDate);
                    cmd.Parameters.AddWithValue("@OfficePhone", objEmp.OfficePhone);
                    cmd.Parameters.AddWithValue("@Mobile", objEmp.Mobile);
                    cmd.Parameters.AddWithValue("@EmployeeTypeId", objEmp.EmployeeTypeId);
                    cmd.Parameters.AddWithValue("@Address", objEmp.Address);
                    cmd.Parameters.AddWithValue("@State", objEmp.State);
                    cmd.Parameters.AddWithValue("@City", objEmp.City);
                    cmd.Parameters.AddWithValue("@PostalCode", objEmp.PostalCode);
                    cmd.Parameters.AddWithValue("@EmergencyContactInfo", objEmp.EmergencyContactInfo);
                    cmd.Parameters.AddWithValue("@ImagePath", objEmp.ImagePath);
                    cmd.Parameters.AddWithValue("@DrivingLicenseNo", objEmp.DrivingLicenseNo);
                    cmd.Parameters.AddWithValue("@LicenseExpiryDate", objEmp.LicenseExpiryDate);
                    cmd.Parameters.AddWithValue("@DriverCertifications", objEmp.DriverCertifications);
                    cmd.Parameters.AddWithValue("@Remarks", objEmp.Remarks);
                    cmd.Parameters.AddWithValue("@TechnicianCertifications", objEmp.TechnicianCertifications);
                    cmd.Parameters.AddWithValue("@AttachmentsPath", objEmp.AttachmentsPath);
                    cmd.Parameters.AddWithValue("@EmployeeCTC", objEmp.EmployeeCTC);
                    cmd.Parameters.AddWithValue("@Qualification", objEmp.Qualification);
                    cmd.Parameters.AddWithValue("@Experience", objEmp.Experience);
                    cmd.Parameters.AddWithValue("@IdProof", objEmp.IdProof);
                    cmd.Parameters.AddWithValue("@EmployeeCode", objEmp.EmployeeCode);
                    cmd.Parameters.AddWithValue("@RoleResponisbility", objEmp.RoleResponisbility);
                    cmd.Parameters.AddWithValue("@BloodGroup", objEmp.BloodGroup);
                    cmd.Parameters.AddWithValue("@PermanentAddress", objEmp.PermanentAddress);
                    cmd.Parameters.AddWithValue("@PermanentState", objEmp.PermanentState);
                    cmd.Parameters.AddWithValue("@PermanentCity", objEmp.PermanentCity);
                    cmd.Parameters.AddWithValue("@PermanentPostalCode", objEmp.PermanentPostalCode);
                    cmd.Parameters.AddWithValue("@ETMNO", objEmp.ETMNo);
                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    con.Close();
                    result = rowsAffected > 0 ? "Employee saved successfully" : "Failed to save employee";
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

        //public string AddUpdateEmployee(Employee objEmp)
        //{
        //    string result = "";

        //    try
        //    {
        //        using (SqlConnection con = new SqlConnection(_FMSConString43))
        //        using (SqlCommand cmd = new SqlCommand("[dbo].[EmpInfoAddUpdate]", con))
        //        {
        //            cmd.CommandType = CommandType.StoredProcedure;

        //            cmd.Parameters.AddWithValue("@EmployeeID", objEmp.EmployeeID);
        //            cmd.Parameters.AddWithValue("@Custid", objEmp.Custid);
        //            cmd.Parameters.AddWithValue("@EmployeeType", objEmp.EmployeeType);
        //            cmd.Parameters.AddWithValue("@contractDuration", objEmp.contractDuration);
        //            cmd.Parameters.AddWithValue("@FirstName", objEmp.FirstName);
        //            cmd.Parameters.AddWithValue("@LastName", objEmp.LastName);
        //            cmd.Parameters.AddWithValue("@HireDate", objEmp.HireDate);
        //            cmd.Parameters.AddWithValue("@OfficePhone", objEmp.OfficePhone);
        //            cmd.Parameters.AddWithValue("@Mobile", objEmp.Mobile);
        //            cmd.Parameters.AddWithValue("@EmployeeTypeId", objEmp.EmployeeTypeId);
        //            cmd.Parameters.AddWithValue("@Address", objEmp.Address);
        //            cmd.Parameters.AddWithValue("@State", objEmp.State);
        //            cmd.Parameters.AddWithValue("@City", objEmp.City);
        //            cmd.Parameters.AddWithValue("@PostalCode", objEmp.PostalCode);
        //            cmd.Parameters.AddWithValue("@EmergencyContactInfo", objEmp.EmergencyContactInfo);
        //            cmd.Parameters.AddWithValue("@ImagePath", objEmp.ImagePath);
        //            cmd.Parameters.AddWithValue("@DrivingLicenseNo", objEmp.DrivingLicenseNo);
        //            cmd.Parameters.AddWithValue("@LicenseExpiryDate", objEmp.LicenseExpiryDate);
        //            cmd.Parameters.AddWithValue("@DriverCertifications", objEmp.DriverCertifications);
        //            cmd.Parameters.AddWithValue("@Remarks", objEmp.Remarks);
        //            cmd.Parameters.AddWithValue("@TechnicianCertifications", objEmp.TechnicianCertifications);
        //            cmd.Parameters.AddWithValue("@AttachmentsPath", objEmp.AttachmentsPath);
        //            cmd.Parameters.AddWithValue("@EmployeeCTC", objEmp.EmployeeCTC);
        //            cmd.Parameters.AddWithValue("@Qualification", objEmp.Qualification);
        //            cmd.Parameters.AddWithValue("@Experience", objEmp.Experience);
        //            cmd.Parameters.AddWithValue("@IdProof", objEmp.IdProof);
        //            cmd.Parameters.AddWithValue("@EmployeeCode", objEmp.EmployeeCode);
        //            cmd.Parameters.AddWithValue("@RoleResponisbility", objEmp.RoleResponisbility);
        //            cmd.Parameters.AddWithValue("@BloodGroup", objEmp.BloodGroup);
        //            cmd.Parameters.AddWithValue("@PermanentAddress", objEmp.PermanentAddress);
        //            cmd.Parameters.AddWithValue("@PermanentState", objEmp.PermanentState);
        //            cmd.Parameters.AddWithValue("@PermanentCity", objEmp.PermanentCity);
        //            cmd.Parameters.AddWithValue("@PermanentPostalCode", objEmp.PermanentPostalCode);
        //            cmd.Parameters.AddWithValue("@ETMNO", objEmp.ETMNo);

        //            con.Open();

        //            // ❌ COMMENTED FOR TESTING (DO NOT SAVE TO DB)
        //            // int rowsAffected = cmd.ExecuteNonQuery();

        //            con.Close();

        //            //  TEST RESPONSE ONLY
        //            result = "Test mode: Employee API hit successfully, DB not executed";
        //        }
        //    }
        //    catch (SqlException ex)
        //    {
        //        result = "SQL Error: " + ex.Message;
        //    }
        //    catch (Exception ex)
        //    {
        //        result = "Error: " + ex.Message;
        //    }

        //    return result;
        //}


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


    }
}
