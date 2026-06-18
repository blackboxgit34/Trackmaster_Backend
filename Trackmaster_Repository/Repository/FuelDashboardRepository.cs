using iText.Commons.Actions.Contexts;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Trackmaster_Model;
using Trackmaster_Repository.Interface;
using static Trackmaster_Repository.DataTypeHelper;

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


        public async Task<List<FuelAnalysisResult>> FuelDisconAnalysisAsync(DateTime beginDate,DateTime endDate,string tblName,string analysisString)
        {
            const string dateFormat = "dd/MMM/yyyy hh:mm:ss  tt";

            if (string.IsNullOrWhiteSpace(tblName) ||
                !Regex.IsMatch(tblName, @"^[a-zA-Z0-9_]+$"))
            {
                throw new ArgumentException("Invalid table name.");
            }

            int targetFuelLevel = analysisString switch
            {
                "Discon" => -1,
                "Garbage" => -340,
                _ => throw new ArgumentException("Invalid AnalysisString.")
            };

            var results = new List<FuelAnalysisResult>();

            bool flag = false;

            DateTime startd = default;
            DateTime endd = default;

            FuelAnalysisResult current = null;

            // ⭐ NEW
            int disconCount = 0;

            // ⭐ NEW
            int garbageCount = 0;

            string query = $@"
        SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

        SELECT
            datadate,
            loc,
            latitude,
            longitude,
            fuellevel
        FROM [{tblName}]
        WHERE datadate BETWEEN @BeginDate AND @EndDate
        ORDER BY datadate ASC";

            await using var con = new SqlConnection(_connectionString43);

            await con.OpenAsync();

            await using var cmd = new SqlCommand(query, con);

            cmd.Parameters.Add("@BeginDate", SqlDbType.DateTime).Value = beginDate;
            cmd.Parameters.Add("@EndDate", SqlDbType.DateTime).Value = endDate;

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                int fuelLevel = GetInt(reader["fuellevel"]);
                DateTime currentDate = GetDateTime(reader["datadate"]);

                if (fuelLevel == targetFuelLevel && !flag)
                {
                    current = new FuelAnalysisResult
                    {
                        StartDate = currentDate.ToString(dateFormat),
                        SLoc = GetString(reader["loc"]),
                        SLat = GetString(reader["latitude"]),
                        SLong = GetString(reader["longitude"])
                    };

                    startd = currentDate;
                    endd = currentDate;

                    flag = true;
                }
                else if (fuelLevel == targetFuelLevel && flag)
                {
                    endd = currentDate;
                }
                else if (flag)
                {
                    current.EndDate = (endd < currentDate ? currentDate : endd)
                        .ToString(dateFormat);

                    if (endd < currentDate)
                    {
                        endd = currentDate;
                    }

                    current.ELoc = GetString(reader["loc"]);
                    current.ELat = GetString(reader["latitude"]);
                    current.ELong = GetString(reader["longitude"]);

                    long totalSeconds = GetInt(
                        endd.Subtract(startd).TotalSeconds);

                    current.Duration = GetElapsedTime1(totalSeconds);
                    current.Duration1 = totalSeconds;

                    results.Add(current);

                    // ⭐ NEW
                    if (targetFuelLevel == -1)
                    {
                        disconCount++;
                    }
                    else
                    {
                        garbageCount++;
                    }

                    current = null;
                    flag = false;
                }
            }

            if (flag && current != null)
            {
                long totalSeconds =GetInt(
                    endd.Subtract(startd).TotalSeconds);

                if (totalSeconds > 0)
                {
                    current.Duration = GetElapsedTime1(totalSeconds);
                    current.Duration1 = totalSeconds;
                }

                results.Add(current);
                // ⭐ NEW
                if (targetFuelLevel == -1)
                {
                    disconCount++;
                }
                else
                {
                    garbageCount++;
                }
            }
            // ⭐ NEW
            results.ForEach(item =>
            {
                item.DisconCount = disconCount;
                item.GarbageCount = garbageCount;
            });

            return results;
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
                days =GetInt(GetFloat(interval / 86400));
                totalhours = GetInt(GetFloat(interval / 3600));
                totalminutes = GetInt(GetFloat(interval / 60));
                totalseconds = GetInt(GetFloat(interval));
                hours = totalhours % 24;
                Minutes = totalminutes % 60;
                Seconds = totalseconds % 60;
                string dayT = GetString(days);
                string hourT = GetString(hours);
                string minT = GetString(Minutes);
                string secT = GetString(Seconds);
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
    }
}   
