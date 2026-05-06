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
        public ReportsRepository(IConfiguration configuration)
        {
            _connectionString43 = configuration.GetConnectionString("DefaultConnection43");
        }
        public VehiclesReport GetConductorInfo(int CustId, int lowerBound, int upperBound, string sSearch)
        {
            var modelObj = new VehiclesReport();
            modelObj.modelObjList = new List<VehicleInformation>();
            if (sSearch == "null" || string.IsNullOrEmpty(sSearch))
            {
                sSearch = null;
            }
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString43))
                {
                    using (SqlCommand cmd = new SqlCommand("[dbo].[GetCrewData]", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@CustId", CustId);
                        cmd.Parameters.AddWithValue("@startRowIndex", lowerBound);
                        cmd.Parameters.AddWithValue("@pageSize", upperBound);
                        cmd.Parameters.AddWithValue("@vehName", string.IsNullOrEmpty(sSearch) ? (object)DBNull.Value : sSearch);
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



    }
}
