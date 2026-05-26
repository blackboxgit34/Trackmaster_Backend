using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

using iText.IO.Image;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;

using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

using System.Data;
using System.Reflection;

namespace Trackmaster_Service.Service
{
    public class ImportExportPdfService
    {
        private readonly string _connectionString43;

        public ImportExportPdfService(
            IConfiguration configuration
        )
        {
            _connectionString43 =
                configuration.GetConnectionString("DefaultConnection43");
        }

        public async Task<byte[]> ExportToPdfFlatList<T>(
            List<T> data,
            string reportName,
            string driverName,
            string vehName
        )
        {
            string bbid;
            string companyName;
            string imagePath;

            try
            {
                var bbiditem = data[0]
                    .GetType()
                    .GetProperties()
                    .Where(x => x.Name.ToLower() == "bbid")
                    .FirstOrDefault();

                bbid =
                    bbiditem?.GetValue(data[0], null)?.ToString();

                var companyInfo =
                    await InitCompanyInfo(bbid);

                companyName = companyInfo.CompanyName;
                imagePath = companyInfo.CompanyLogo;
            }
            catch
            {
                companyName = "Blackbox";
                imagePath = "black-box-logo.png";
            }

            using var stream = new MemoryStream();

            PdfWriter writer =
                new PdfWriter(stream);

            PdfDocument pdf =
                new PdfDocument(writer);

            // LANDSCAPE MODE
            pdf.SetDefaultPageSize(
                PageSize.A4.Rotate()
            );

            Document document =
                new Document(pdf);

            // ================= LOGO =================

            var logoPath = System.IO.Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                imagePath
            );

            if (!File.Exists(logoPath))
            {
                logoPath = System.IO.Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "black-box-logo.png"
                );
            }

            if (File.Exists(logoPath))
            {
                ImageData imageData =
                    ImageDataFactory.Create(logoPath);

                var image =
                    new Image(imageData);

                image.SetWidth(120);

                document.Add(image);
            }

            // ================= REPORT NAME =================

            document.Add(
      new Paragraph(
          "Report Name : " + reportName
      )
      .SetFontSize(14)
  );

            // ================= DATE =================

            document.Add(
                new Paragraph(
                    "Date : " +
                    DateTime.Now.ToString("dd/MM/yyyy")
                )
                .SetFontSize(11)
            );

            // ================= DRIVER / VEHICLE =================

            if (
                !string.IsNullOrEmpty(driverName)
                || !string.IsNullOrEmpty(vehName)
            )
            {
                document.Add(
                    new Paragraph(
                        "Driver Name : " + driverName
                    )
                    .SetFontSize(11)
                );

                document.Add(
                    new Paragraph(
                        "Vehicle Name : " + vehName
                    )
                    .SetFontSize(11)
                );
            }

            document.Add(new Paragraph(" "));

            // ================= TABLE =================

            if (data != null && data.Count > 0)
            {
                var properties =
                    typeof(T).GetProperties();

                // AUTO WIDTH TABLE
                float[] columnWidths =
                    Enumerable
                    .Repeat(1f, properties.Length)
                    .ToArray();

                Table table =
                    new Table(columnWidths);

                table.SetWidth(
                    UnitValue.CreatePercentValue(100)
                );

                // ================= HEADER =================

                foreach (PropertyInfo prop in properties)
                {
                    table.AddHeaderCell(
                        new Cell().Add(
                            new Paragraph(prop.Name)
                                .SetFontSize(9)
                        )
                    );
                }
                // ================= DATA =================

                foreach (var item in data)
                {
                    foreach (PropertyInfo prop in properties)
                    {
                        var value =
                            prop.GetValue(item);

                        table.AddCell(
                            new Cell().Add(
                                new Paragraph(
                                    value?.ToString() ?? ""
                                )
                                .SetFontSize(8)
                            )
                        );
                    }
                }

                document.Add(table);
            }

            document.Add(new Paragraph(" "));

            // ================= FOOTER =================

            document.Add(
                new Paragraph(
                    "Generated on : "
                    + DateTime.Now.ToString("dd/MM/yyyy hh:mm tt")
                    + " by "
                    + companyName
                )
                .SetFontSize(8)
            );

            document.Close();

            return await Task.FromResult(
                stream.ToArray()
            );
        }

        // ================= COMPANY INFO =================

        public class CompanyInfo
        {
            public string CompanyName { get; set; }

            public string CompanyLogo { get; set; }
        }

        private async Task<CompanyInfo> InitCompanyInfo(
            string bbid
        )
        {
            var companyInfo =
                new CompanyInfo
                {
                    CompanyName = "blackbox",
                    CompanyLogo = "black-box-logo.png"
                };

            using var con =
                new SqlConnection(_connectionString43);

            using var cmd =
                new SqlCommand(
                    "getCompanyDetails",
                    con
                );

            cmd.CommandType =
                CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue(
                "@bbid",
                bbid
            );

            await con.OpenAsync();

            using var reader =
                await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                companyInfo.CompanyName =
                    reader["CompanyName"] == DBNull.Value
                    ? "blackbox"
                    : Convert.ToString(
                        reader["CompanyName"]
                    );

                companyInfo.CompanyLogo =
                    reader["CompanyLogo"] == DBNull.Value
                    ? "black-box-logo.png"
                    : Convert.ToString(
                        reader["CompanyLogo"]
                    );
            }

            return companyInfo;
        }
    }
}