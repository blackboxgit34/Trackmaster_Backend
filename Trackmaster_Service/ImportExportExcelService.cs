/* To work with EPPlus library */
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Style;
using System.Collections;
using System.Data;
using System.Drawing;
using System.Text.RegularExpressions;

namespace Trackmaster_Service
{
    public class ImportExportExcelService
    {
        private readonly string _connectionString43;

        public ImportExportExcelService(
            IConfiguration configuration
        )
        {
            _connectionString43 = configuration.GetConnectionString("DefaultConnection43");
        }

        public static int Pixel2MTU(int pixels)
        {
            int mtus = pixels * 19525;
            return mtus;
        }

        static string GetExcelColumnName(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }

        public async Task<byte[]> ExportToExcelFlatList<T>(
            List<T> test,
            string reportName,
            string driverName,
            string vehName
        )
        {
            string bbid, companyName, imagePath;

            try
            {
                var bbiditem = test[0]
                    .GetType()
                    .GetProperties()
                    .Where(x => x.Name.ToLower() == "bbid")
                    .ToList()[0];

                bbid = bbiditem.GetValue(test[0], null).ToString();

                var companyInfo = await InitCompanyInfo(bbid);

                companyName = companyInfo.CompanyName;
                imagePath = companyInfo.CompanyLogo;
            }
            catch (Exception)
            {
                companyName = "Blackbox";
                imagePath = "black-box-logo.png";
            }

            int counter = 0;
            var dataList = test.ToList();
            var count = dataList.Count;
            Type type = typeof(T);
            var LengthOfClassAttributes = type.GetProperties().Length;
            int borderCol = 1;

            using (var excelPacakge = new ExcelPackage())
            {
                #region For giving name, title and these kind of info manually

                excelPacakge.Workbook.Properties.Author = companyName;
                excelPacakge.Workbook.Properties.Title = companyName;
                excelPacakge.Workbook.Properties.Comments =
                    companyName + " generated excel file";

                var ws = excelPacakge.Workbook.Worksheets
                    .FirstOrDefault(x => x.Name == "Content");

                if (ws == null)
                {
                    ws = excelPacakge.Workbook.Worksheets.Add("Content");
                }

                #endregion

                var worksheet = ws;

                //How to Add an Image using EPPlus
                var ffname = imagePath;
                var logoName = "";
                logoName = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    ffname
                );

                if (!File.Exists(logoName))
                {
                    logoName = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "black-box-logo.png"
                    );
                }

                FileInfo fileInfo = new FileInfo(logoName);

                ExcelPicture picture = ws.Drawings.AddPicture(
                    "Debopam Pal",
                    fileInfo
                );

                picture.SetSize(100, 20);

                picture.From.ColumnOff = Pixel2MTU(2);
                picture.From.RowOff = Pixel2MTU(2);
                ws.Cells[2, 1].Value = "Report Name: " + reportName;
                ws.Cells[2, 6].Value = "Date: " + DateTime.Now.ToShortDateString();

                ws.Cells[2, 1, 2, 5].Merge = true;
                ws.Cells[2, 6, 2, 6].Merge = true;

                ws.Cells[2, 1, 2, 5].Style.Font.Bold = true;

                ws.Cells[2, 1, 2, 5].Style.HorizontalAlignment =
                    ExcelHorizontalAlignment.Left;

                var rowNo = 4;

                if (!string.IsNullOrEmpty(vehName)
                    || !string.IsNullOrEmpty(driverName))
                {
                    rowNo++;

                    ws.Cells[3, 1].Value = "Driver Name: " + driverName;
                    ws.Cells[3, 6].Value = "Vehicle Name: " + vehName;

                    ws.Cells[3, 1, 3, 5].Merge = true;
                    ws.Cells[3, 6, 3, 5].Merge = true;

                    ws.Cells[3, 1, 3, 5].Style.Font.Bold = true;

                    ws.Cells[3, 1, 3, 5].Style.HorizontalAlignment =
                        ExcelHorizontalAlignment.Left;
                }

                int numberOfRows = dataList.Count;
                int parentColNo = 1;
                bool parentHeader = true;
                bool containsChildren = false;

                foreach (var outputColumns in dataList)
                {
                    parentColNo = 1;
                    int childColNo = 2;

                    if (parentHeader)
                    {
                        foreach (var item in outputColumns.GetType().GetProperties())
                        {
                            string displayValue =
                                (item.CustomAttributes.Count() > 0)
                                ? ((item.CustomAttributes.FirstOrDefault()
                                    .NamedArguments.Count > 0)
                                    ? item.CustomAttributes.FirstOrDefault()
                                        .NamedArguments.FirstOrDefault()
                                        .MemberName
                                    : string.Empty)
                                : string.Empty;

                            string isHidden =
                                (item.CustomAttributes.Count() > 0)
                                ? ((item.CustomAttributes.FirstOrDefault()
                                    .NamedArguments.Count > 0)
                                    ? item.CustomAttributes.FirstOrDefault()
                                        .NamedArguments.FirstOrDefault()
                                        .TypedValue.Value.ToString()
                                    : string.Empty)
                                : string.Empty;

                            if (displayValue.ToLower().Equals("displayvalue")
                                && isHidden.ToLower().Equals("false"))
                            {
                            }
                            else
                            {
                                if (item.PropertyType.IsGenericType)
                                {
                                    //Do Not show header for list data
                                }
                                else
                                {
                                    worksheet.Cells[rowNo, parentColNo].Value =
                                        (item.CustomAttributes.Count() > 0)
                                        ? item.CustomAttributes.FirstOrDefault()
                                            .ConstructorArguments.FirstOrDefault()
                                            .Value
                                        : item.Name;

                                    worksheet.Cells[rowNo, parentColNo]
                                        .Style.Fill.PatternType =
                                        ExcelFillStyle.Solid;

                                    Color colFromHex1 =
                                        ColorTranslator.FromHtml("#d3d3d3");

                                    worksheet.Cells[rowNo, parentColNo]
                                        .Style.Fill.BackgroundColor
                                        .SetColor(colFromHex1);

                                    string values = item.Name;

                                    if (values != null)
                                    {
                                        if (!string.IsNullOrEmpty(values.ToString()))
                                        {
                                            if (values.ToString().Length > 30)
                                            {
                                                worksheet.Column(parentColNo).Width = 80;
                                            }
                                            else if (values.ToString().Length > 0
                                                && values.ToString().Length <= 5)
                                            {
                                                worksheet.Column(parentColNo).Width = 5;
                                            }
                                            else if (values.ToString().Length >= 5
                                                && values.ToString().Length <= 11)
                                            {
                                                worksheet.Column(parentColNo).Width = 18;
                                            }
                                            else
                                            {
                                                worksheet.Column(parentColNo).Width = 20;
                                            }
                                        }
                                    }

                                    parentColNo++;
                                }
                            }
                        }

                        parentHeader = false;
                        rowNo++;
                    }

                    parentColNo = 1;

                    bool childHeader = true;

                    foreach (var item in outputColumns.GetType().GetProperties())
                    {
                        if (item.PropertyType.IsGenericType)
                        {
                            if (item.GetValue(outputColumns, null) != null)
                            {
                                containsChildren = true;

                                foreach (var data in item.GetValue(outputColumns, null) as IEnumerable)
                                {
                                    rowNo++;

                                    IList collection =
                                        data.GetType().GetProperties().ToList();

                                    if (childHeader)
                                    {
                                        foreach (var header in data.GetType().GetProperties())
                                        {
                                            string displayValue =
                                                (header.CustomAttributes.Count() > 0)
                                                ? ((header.CustomAttributes.FirstOrDefault()
                                                    .NamedArguments.Count > 0)
                                                    ? header.CustomAttributes
                                                        .FirstOrDefault()
                                                        .NamedArguments
                                                        .FirstOrDefault()
                                                        .MemberName
                                                    : string.Empty)
                                                : string.Empty;

                                            string isHidden =
                                                (header.CustomAttributes.Count() > 0)
                                                ? ((header.CustomAttributes
                                                    .FirstOrDefault()
                                                    .NamedArguments.Count > 0)
                                                    ? header.CustomAttributes
                                                        .FirstOrDefault()
                                                        .NamedArguments
                                                        .FirstOrDefault()
                                                        .TypedValue.Value
                                                        .ToString()
                                                    : string.Empty)
                                                : string.Empty;

                                            if (displayValue.ToLower().Equals("displayvalue")
                                                && isHidden.ToLower().Equals("false"))
                                            {
                                            }
                                            else
                                            {
                                                if (counter == 0)
                                                {
                                                    worksheet.Cells[rowNo, childColNo].Value =
                                                        (header.CustomAttributes.Count() > 0)
                                                        ? header.CustomAttributes
                                                            .FirstOrDefault()
                                                            .ConstructorArguments
                                                            .FirstOrDefault()
                                                            .Value
                                                        : header.Name;

                                                    worksheet.Cells[rowNo, childColNo]
                                                        .Style.Fill.PatternType =
                                                        ExcelFillStyle.Solid;

                                                    Color colFromHex =
                                                        ColorTranslator.FromHtml("#b7dbff");

                                                    worksheet.Cells[rowNo, childColNo]
                                                        .Style.Fill.BackgroundColor
                                                        .SetColor(colFromHex);

                                                    childColNo++;
                                                }
                                            }
                                        }

                                        childHeader = false;

                                        if (counter == 0)
                                        {
                                            rowNo++;
                                        }

                                        counter++;
                                    }

                                    childColNo = 2;

                                    foreach (var dataItem in collection)
                                    {
                                        var list =
                                            ((System.Reflection.PropertyInfo)(dataItem))
                                            .CustomAttributes.ToList();

                                        int cnt = list.Count;

                                        string displayValue = string.Empty;
                                        string isHidden = string.Empty;

                                        if (cnt > 0)
                                        {
                                            var namedAttrList =
                                                ((System.Reflection.PropertyInfo)(dataItem))
                                                .CustomAttributes
                                                .FirstOrDefault()
                                                .NamedArguments
                                                .ToList();

                                            int namedAttrListCnt = namedAttrList.Count;

                                            if (namedAttrListCnt > 0)
                                            {
                                                displayValue =
                                                    ((System.Reflection.PropertyInfo)(dataItem))
                                                    .CustomAttributes
                                                    .FirstOrDefault()
                                                    .NamedArguments
                                                    .FirstOrDefault()
                                                    .MemberName
                                                    .ToString();

                                                isHidden =
                                                    ((System.Reflection.PropertyInfo)(dataItem))
                                                    .CustomAttributes
                                                    .FirstOrDefault()
                                                    .NamedArguments
                                                    .FirstOrDefault()
                                                    .TypedValue.Value
                                                    .ToString();
                                            }
                                        }

                                        if (displayValue.ToLower().Equals("displayvalue")
                                            && isHidden.ToLower().Equals("false"))
                                        {
                                        }
                                        else
                                        {
                                            string value =
                                                (((System.Reflection.PropertyInfo)(dataItem))
                                                .GetValue(data) == null)
                                                ? string.Empty
                                                : ((System.Reflection.PropertyInfo)(dataItem))
                                                    .GetValue(data)
                                                    .ToString();

                                            value = Regex.Replace(value, "<.*?>", String.Empty);

                                            worksheet.Cells[rowNo, childColNo].Value = value;

                                            if (!string.IsNullOrEmpty(value))
                                            {
                                                if (value.Length > 30)
                                                {
                                                    worksheet.Column(childColNo).Width = 80;
                                                }
                                                else if (value.Length >= 2
                                                    && value.Length <= 11)
                                                {
                                                    worksheet.Column(childColNo).Width = 18;
                                                }
                                                else
                                                {
                                                    worksheet.Column(childColNo).Width = 23;
                                                }
                                            }

                                            childColNo++;
                                        }
                                    }
                                }

                                borderCol =
                                    Math.Max(
                                        Math.Max(parentColNo, childColNo),
                                        borderCol
                                    );
                            }
                        }
                        else
                        {
                            var list =
                                ((System.Reflection.PropertyInfo)(item))
                                .CustomAttributes.ToList();

                            int cnt = list.Count;

                            string displayValue = string.Empty;
                            string isHidden = string.Empty;

                            if (cnt > 0)
                            {
                                var namedAttrList =
                                    ((System.Reflection.PropertyInfo)(item))
                                    .CustomAttributes
                                    .FirstOrDefault()
                                    .NamedArguments
                                    .ToList();

                                int namedAttrListCnt = namedAttrList.Count;

                                if (namedAttrListCnt > 0)
                                {
                                    displayValue =
                                        ((System.Reflection.PropertyInfo)(item))
                                        .CustomAttributes
                                        .FirstOrDefault()
                                        .NamedArguments
                                        .FirstOrDefault()
                                        .MemberName
                                        .ToString();

                                    isHidden =
                                        ((System.Reflection.PropertyInfo)(item))
                                        .CustomAttributes
                                        .FirstOrDefault()
                                        .NamedArguments
                                        .FirstOrDefault()
                                        .TypedValue.Value
                                        .ToString();
                                }
                            }

                            if (displayValue.ToLower().Equals("displayvalue")
                                && isHidden.ToLower().Equals("false"))
                            {
                            }
                            else
                            {
                                var values = item.GetValue(outputColumns, null);

                                worksheet.Cells[rowNo, parentColNo].Value =
                                    item.GetValue(outputColumns, null);

                                worksheet.Cells[rowNo, parentColNo]
                                    .Style.Fill.PatternType =
                                    ExcelFillStyle.Solid;

                                Color colFromHex =
                                    ColorTranslator.FromHtml("#e4f4e4");

                                worksheet.Cells[rowNo, parentColNo]
                                    .Style.Fill.BackgroundColor
                                    .SetColor(colFromHex);

                                try
                                {
                                    if (values != null)
                                    {
                                        if (!string.IsNullOrEmpty(values.ToString()))
                                        {
                                            if (values.ToString().Length > 30)
                                            {
                                                worksheet.Column(parentColNo).Width = 80;
                                            }
                                            else if (values.ToString().Length > 0
                                                && values.ToString().Length <= 5)
                                            {
                                                worksheet.Column(parentColNo).Width = 13;
                                            }
                                            else if (values.ToString().Length >= 5
                                                && values.ToString().Length <= 11)
                                            {
                                                worksheet.Column(parentColNo).Width = 18;
                                            }
                                            else
                                            {
                                                worksheet.Column(parentColNo).Width = 20;
                                            }
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                }

                                parentColNo++;
                            }
                        }
                    }

                    rowNo++;
                }

                worksheet.DefaultColWidth = 15;
                worksheet.Cells.Style.WrapText = true;

                if (containsChildren)
                {
                    borderCol = borderCol - 1;
                }
                else
                {
                    borderCol = parentColNo;
                    borderCol = borderCol - 1;
                }

                var rangeUpto = GetExcelColumnName(borderCol);

                if (borderCol > 0)
                {
                    using (var range =
                        worksheet.Cells["A" + rowNo + ":" + rangeUpto + rowNo])
                    {
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;

                        range.Style.Fill.BackgroundColor
                            .SetColor(Color.LightGray);

                        range.Style.Font.Color
                            .SetColor(Color.White);

                        ws.Cells[rowNo, 1, rowNo, borderCol]
                            .Style.Font.Bold = true;

                        ws.Cells[rowNo, 1, rowNo, borderCol]
                            .Style.HorizontalAlignment =
                            ExcelHorizontalAlignment.Left;
                    }

                    using (var range2 =
                        worksheet.Cells["A4" + ":" + rangeUpto + (rowNo - 1)])
                    {
                        range2.Style.Border.Top.Style =
                            ExcelBorderStyle.Thin;

                        range2.Style.Border.Bottom.Style =
                            ExcelBorderStyle.Thin;

                        range2.Style.Border.Left.Style =
                            ExcelBorderStyle.Thin;

                        range2.Style.Border.Right.Style =
                            ExcelBorderStyle.Thin;
                    }

                    ws.Cells[rowNo + 1, 1].Value =
                        "Generated on : "
                        + DateTime.Now
                        + " by "
                        + companyName
                        + ".";

                    ws.Cells[rowNo + 1, 1, rowNo + 1, borderCol].Merge = true;

                    ws.Cells[rowNo + 1, 1, rowNo + 1, borderCol]
                        .Style.Font.Italic = true;

                    ws.Cells[rowNo + 1, 1, rowNo + 1, borderCol]
                        .Style.Font.Size = 8;

                    ws.Cells[rowNo + 1, 1, rowNo + 1, borderCol]
                        .Style.HorizontalAlignment =
                        ExcelHorizontalAlignment.Left;
                }

                var stream = excelPacakge.GetAsByteArray();

                return stream;
            }
        }

        static void UpdateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public class CompanyInfo
        {
            public string CompanyName { get; set; }
            public string CompanyLogo { get; set; }
        }

        private async Task<CompanyInfo> InitCompanyInfo(string bbid)
        {
            var companyInfo = new CompanyInfo
            {
                CompanyName = "blackbox",
                CompanyLogo = "black-box-logo.png"
            };

            try
            {
                using var con = new SqlConnection(_connectionString43);

                using var cmd = new SqlCommand(
                    "getCompanyDetails",
                    con
                );

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@bbid", bbid);

                await con.OpenAsync();

                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    companyInfo.CompanyName =
                        reader["CompanyName"] == DBNull.Value
                        ? "blackbox"
                        : Convert.ToString(reader["CompanyName"]);

                    companyInfo.CompanyLogo =
                        reader["CompanyLogo"] == DBNull.Value
                        ? "black-box-logo.png"
                        : Convert.ToString(reader["CompanyLogo"]);
                }
            }
            catch (Exception)
            {
                throw;
            }

            return companyInfo;
        }
    }
}