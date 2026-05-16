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
        public ReportsRepository(IConfiguration configuration)
        {
            _connectionString43 = configuration.GetConnectionString("DefaultConnection43");
            _FMSConString43 = configuration.GetConnectionString("FMSConString43");
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
                    cmd.Parameters.AddWithValue("@ImagePath",string.IsNullOrEmpty(imagePaths) ? "" : imagePaths);

                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();con.Close();
                    result = rowsAffected > 0? "Employee saved successfully": "Failed to save employee";
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






    }
}
