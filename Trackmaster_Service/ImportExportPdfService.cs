using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Reflection;

namespace Trackmaster_Service
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

            pdf.SetDefaultPageSize(
                PageSize.A3.Rotate()
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
                 new Paragraph(reportName)
                .SetFontSize(14)
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                .SetTextAlignment(TextAlignment.CENTER)
            );


            // ================= DATE =================

            document.Add(
                new Paragraph(
                    "Date : " + DateTime.Now.ToString("dd/MM/yyyy")
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
                var properties = typeof(T).GetProperties();

                foreach (var parent in data)
                {

                    var parentProps = properties
                    .Where(p => !IsEnumerableButNotString(p.GetValue(parent)))
                    .ToArray();
                    bool isWideParentTable = parentProps.Length > 10;
                    Table parentTable = isWideParentTable
                        ? new Table(2).UseAllAvailableWidth()
                        : new Table(parentProps.Length).UseAllAvailableWidth();

                   

                    if (!isWideParentTable)
                    {
                        foreach (var prop in parentProps)
                        {
                            parentTable.AddHeaderCell(
                                new Cell().Add(
                                    new Paragraph(prop.Name)
                                    .SetFontSize(11)
                                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                                )
                            );
                        }
                    }

                   

                    if (isWideParentTable)
                    {
                      
                        parentTable.AddHeaderCell(
                        new Cell().Add(
                        new Paragraph("Property")
                            .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                            )
                        );

                        parentTable.AddHeaderCell(
                            new Cell().Add(
                                new Paragraph("Value")
                                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                            )
                        );

                        foreach (var prop in parentProps)
                        {
                            var value = prop.GetValue(parent);

                            parentTable.AddCell(prop.Name);

                            parentTable.AddCell(
                                new Cell()
                                .Add(
                                    new Paragraph(value?.ToString() ?? "")
                                    //new Paragraph((value?.ToString() ?? "").Replace(" ", "\u00A0"))
                                    .SetFontSize(11)
                                )
                            );
                        }
                    }
                    else
                    {
                        foreach (var prop in parentProps)
                        {
                            var value = prop.GetValue(parent);

                            parentTable.AddCell(
                                new Cell()
                                .Add(
                                    new Paragraph(value?.ToString() ?? "")
                                    //new Paragraph((value?.ToString() ?? "").Replace(" ", "\u00A0"))
                                    .SetFontSize(11)
                                )
                                .SetKeepTogether(false)
                            );
                        }


                    }

                    document.Add(parentTable);

                    // ================= CHILD TABLE =================
                    foreach (var prop in properties)
                    {
                        var value = prop.GetValue(parent);

                        if (!IsEnumerableButNotString(value))
                            continue;

                        var list = value as System.Collections.IEnumerable;

                        var firstItem = list.Cast<object>().FirstOrDefault();

                        if (firstItem == null)
                            continue;

                        var childProps = firstItem.GetType().GetProperties();

                        bool isWideChildTable = childProps.Length > 10;

                        Table childTable = isWideChildTable
                            ? new Table(2).UseAllAvailableWidth()
                            : new Table(childProps.Length).UseAllAvailableWidth();

                        bool headerAdded = false;

                        foreach (var item in list)
                        {

                            if (!headerAdded)
                            {
                               
                                    if (isWideChildTable)
                                    {
                                        childTable.AddHeaderCell(
                                            new Cell().Add(
                                                new Paragraph("Property")
                                                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                                            )
                                        );

                                        childTable.AddHeaderCell(
                                            new Cell().Add(
                                                new Paragraph("Value")
                                                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                                            )
                                        );
                                    }
                               
                                else
                                {
                                    foreach (var cp in childProps)
                                    {
                                        childTable.AddHeaderCell(
                                            new Cell().Add(
                                                new Paragraph(cp.Name)
                                                .SetFontSize(10)
                                                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                                            )
                                        );
                                    }
                                }

                                headerAdded = true;
                            }

                            if (isWideChildTable)
                            {
                                foreach (var cp in childProps)
                                {
                                    var childValue = cp.GetValue(item);

                                    childTable.AddCell(cp.Name);

                                    childTable.AddCell(
                                        new Cell()
                                        .Add(
                                            new Paragraph(childValue?.ToString() ?? "")
                                            //new Paragraph((childValue?.ToString() ?? "").Replace(" ", "\u00A0"))
                                            .SetFontSize(10)
                                        )
                                    );
                                }
                            }
                            else
                            {
                                foreach (var cp in childProps)
                                {
                                    var childValue = cp.GetValue(item);

                                    childTable.AddCell(
                                        new Cell()
                                        .Add(
                                            new Paragraph(childValue?.ToString() ?? "")
                                           //new Paragraph((childValue?.ToString() ?? "").Replace(" ", "\u00A0"))
                                           //.SetFontSize(10)
                                            .SetFontSize(10)
                                        )
                                        .SetKeepTogether(false)
                                    );
                                }
                            }

                        }

                        document.Add(childTable);
                    }

                    document.Add(new Paragraph("\n"));
                }
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

        //06.06.2026
        private bool IsEnumerableButNotString(object value)
        {
            return value != null &&
                   value is System.Collections.IEnumerable &&
                   !(value is string);
        }

    }
}