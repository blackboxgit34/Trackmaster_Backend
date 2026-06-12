using iText.Commons.Actions.Contexts;
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


namespace Trackmaster_Repository.Repository
{
    public  class FuelDashboardRepository : IFuelDashboardRepository
    {
        private readonly string _connectionString43;
        
        public FuelDashboardRepository(IConfiguration configuration)
        {

            _connectionString43 = configuration.GetConnectionString("DefaultConnection43");
           
        }
        public async Task<FuelDashboardModel> GetCurrentFuelData(int custid)
        {
            FuelDashboardModel model = new FuelDashboardModel();

            try
            {
                int totalCnt = 0;
                int lowLevel = 0;
                int normalFuel = 0;

                

                using (SqlConnection con = new SqlConnection(_connectionString43))
                {
                    await con.OpenAsync();

                    DataTable dt = new DataTable();

                    using (SqlCommand cmd = new SqlCommand("[dbo].[GetGeneratorList]", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Custid", custid);

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }

                    if (dt.Rows.Count == 0)
                        return model;

                    totalCnt = dt.Rows.Count;

                    foreach (DataRow row in dt.Rows)
                    {
                        string bbid = Convert.ToString(row["GenID"]);

                        decimal lowLevelValue = 0;

                        using (SqlCommand cmd = new SqlCommand(@"
                        SELECT ISNULL(LowLevelVal,0)
                        FROM RecommendedConsumption
                        WHERE BoxID=@BBID AND IsActive=1", con))
                        {
                            cmd.Parameters.AddWithValue("@BBID", bbid);

                            object obj = await cmd.ExecuteScalarAsync();

                            if (obj != null && obj != DBNull.Value)
                                lowLevelValue = Convert.ToDecimal(obj);
                        }

                        if (lowLevelValue <= 0)
                        {
                            normalFuel++;
                            continue;
                        }

                        double Diameter = 0;
                        double Length = 0;
                        double Height = 0;
                        double Width = 0;
                        double FreqMin = 0;
                        double FreqMax = 0;
                        double TotalLevel = 0;
                        double FuelLevelinLiters = 0;
                        double LastValue = 0;
                        DateTime LastDateTime = DateTime.Now;

                        int ShapeType = 0;

                        using (SqlCommand cmd = new SqlCommand(
                            "SELECT ISNULL(ShapeType,0) FROM FuelTankShape WHERE BBID=@BBID", con))
                        {
                            cmd.Parameters.AddWithValue("@BBID", bbid);

                            object obj = await cmd.ExecuteScalarAsync();

                            if (obj != null && obj != DBNull.Value)
                                ShapeType = Convert.ToInt32(obj);
                        }

                        if (ShapeType == 1)
                        {
                            using (SqlCommand cmd = new SqlCommand(@"
                            SELECT TOP 1
                                   Diameter,
                                   Length,
                                   MinFreq,
                                   MaxFreq,
                                   LevelInLiters
                            FROM FuelRound
                            WHERE BBID=@BBID", con))
                            {
                                cmd.Parameters.AddWithValue("@BBID", bbid);

                                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                                {
                                    if (await dr.ReadAsync())
                                    {
                                        Diameter = Convert.ToDouble(dr["Diameter"]);
                                        Length = Convert.ToDouble(dr["Length"]);
                                        FreqMin = Convert.ToDouble(dr["MinFreq"]);
                                        FreqMax = Convert.ToDouble(dr["MaxFreq"]);
                                        TotalLevel = Convert.ToDouble(dr["LevelInLiters"]);
                                    }
                                }
                            }

                            string currentQuery = $@"
                        SELECT TOP 1 FuelLevel,DataDate
                        FROM [{bbid}]
                        WHERE VehBatVoltage > 8
                          AND FuelLevel >= {FreqMin}
                          AND FuelLevel <= {FreqMax}
                          AND DataDate >= DATEADD(MINUTE,-20,GETDATE())
                        ORDER BY DataDate DESC";

                            using (SqlCommand cmd = new SqlCommand(currentQuery, con))
                            {
                                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                                {
                                    if (await dr.ReadAsync())
                                    {
                                        double fuelLevel = Convert.ToDouble(dr["FuelLevel"]);

                                        FuelLevelinLiters =
                                            getRoundFinalValue(
                                                fuelLevel,
                                                FreqMax,
                                                FreqMin,
                                                Diameter,
                                                TotalLevel,
                                                Length);
                                    }
                                }
                            }

                            string lastQuery = $@"
                        SELECT TOP 1 FuelLevel,DataDate
                        FROM [{bbid}]
                        WHERE VehBatVoltage > 8
                          AND FuelLevel >= {FreqMin}
                        ORDER BY DataDate DESC";

                            using (SqlCommand cmd = new SqlCommand(lastQuery, con))
                            {
                                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                                {
                                    if (await dr.ReadAsync())
                                    {
                                        double fuelLevel = Convert.ToDouble(dr["FuelLevel"]);

                                        LastDateTime = Convert.ToDateTime(dr["DataDate"]);

                                        LastValue =
                                           getRoundFinalValue(
                                                fuelLevel,
                                                FreqMax,
                                                FreqMin,
                                                Diameter,
                                                TotalLevel,
                                                Length);
                                    }
                                }
                            }
                        }
                        else
                        {
                            using (SqlCommand cmd = new SqlCommand(@"
                            SELECT TOP 1
                                   Height,
                                   Length,
                                   Width,
                                   MinFreq,
                                   MaxFreq,
                                   LevelInLiters
                            FROM FuelRectangle
                            WHERE BBID=@BBID", con))
                            {
                                cmd.Parameters.AddWithValue("@BBID", bbid);

                                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                                {
                                    if (await dr.ReadAsync())
                                    {
                                        Height = Convert.ToDouble(dr["Height"]);
                                        Length = Convert.ToDouble(dr["Length"]);
                                        Width = Convert.ToDouble(dr["Width"]);
                                        FreqMin = Convert.ToDouble(dr["MinFreq"]);
                                        FreqMax = Convert.ToDouble(dr["MaxFreq"]);
                                        TotalLevel = Convert.ToDouble(dr["LevelInLiters"]);
                                    }
                                }
                            }

                            string currentQuery = $@"
                        SELECT TOP 1 FuelLevel,DataDate
                        FROM [{bbid}]
                        WHERE VehBatVoltage > 8
                          AND FuelLevel >= {FreqMin}
                          AND FuelLevel <= {FreqMax}
                          AND DataDate >= DATEADD(MINUTE,-20,GETDATE())
                        ORDER BY DataDate DESC";

                            using (SqlCommand cmd = new SqlCommand(currentQuery, con))
                            {
                                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                                {
                                    if (await dr.ReadAsync())
                                    {
                                        double fuelLevel = Convert.ToDouble(dr["FuelLevel"]);

                                        FuelLevelinLiters =
                                            getRectangularFinalValue(
                                                fuelLevel,
                                                FreqMin,
                                                FreqMax,
                                                Height,
                                                Width,
                                                Length,
                                                TotalLevel);
                                    }
                                }
                            }

                            string lastQuery = $@"
                        SELECT TOP 1 FuelLevel,DataDate
                        FROM [{bbid}]
                        WHERE VehBatVoltage > 8
                          AND FuelLevel >= {FreqMin}
                          AND FuelLevel <= {FreqMax}
                        ORDER BY DataDate DESC";

                            using (SqlCommand cmd = new SqlCommand(lastQuery, con))
                            {
                                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                                {
                                    if (await dr.ReadAsync())
                                    {
                                        double fuelLevel = Convert.ToDouble(dr["FuelLevel"]);

                                        LastDateTime = Convert.ToDateTime(dr["DataDate"]);

                                        LastValue =
                                            getRectangularFinalValue(
                                                fuelLevel,
                                                FreqMin,
                                                FreqMax,
                                                Height,
                                                Width,
                                                Length,
                                                TotalLevel);
                                    }
                                }
                            }
                        }

                        decimal fl = FuelLevelinLiters == 0
                            ? Math.Round(Convert.ToDecimal(LastValue), 2)
                            : Convert.ToDecimal(FuelLevelinLiters);

                        if (fl <= lowLevelValue)
                            lowLevel++;
                        else
                            normalFuel++;
                    }
                }

                model.totalGenset = totalCnt;
                model.lowLevel = lowLevel;
                model.normalLevel = normalFuel;
                model.Message = "Success";
            }
            catch (Exception ex)
            {
                model.totalGenset = 0;
                model.lowLevel = 0;
                model.normalLevel = 0;
                model.Message = ex.Message;
            }

            return model;
        }
        private double getRoundFinalValue(double Fuellevel, double FreqMax, double FreqMin, double Diameter, double totalLevel, double Length)
        {
            double value1 = Fuellevel - FreqMin;
            double value2 = FreqMax - FreqMin;
            double Level = (value1 / value2) * Diameter;
            double Angle = Math.Acos(((Diameter / 2) - Level) / (Diameter / 2));
            double Volume1 = (Angle / 3.1428) * ((3.1428 * Diameter * Diameter * Length) / (4 * 1000000));
            double Volume2 = ((((Diameter / 2) - Level) * Math.Tan(Angle)) * ((Diameter / 2) - Level) * Length) / 1000000;
            double finallevel = 1000 * (Volume1 - Volume2);
            if (finallevel < totalLevel)
            {
                finallevel = Math.Round(finallevel, 0);
            }
            else
            {
                finallevel = Math.Round(totalLevel, 0);
            }
            return finallevel;
        }

        private double getRectangularFinalValue(double Fuellevel, double FreqMin, double FreqMax, double Height, double Width, double Length, double totalLevel)
        {
            double Level = ((Fuellevel - FreqMin) / (FreqMax - FreqMin)) * Height;
            double finallevel = (1000 * Level * Length * Width) / 1000000;
            if (finallevel < totalLevel)
            {
                finallevel = Math.Round(finallevel, 0);
            }
            else
            {
                finallevel = Math.Round(totalLevel, 0);
            }
            return finallevel;
        }
    }
}
