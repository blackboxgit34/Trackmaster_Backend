using OfficeOpenXml;
using Trackmaster_Repository.Interface;
using Trackmaster_Repository.Repository;
using Trackmaster_Service;
using Trackmaster_Service.Interface;
using Trackmaster_Service.Repository;
using Trackmaster_Service.Service;

var builder = WebApplication.CreateBuilder(args);
ExcelPackage.License.SetNonCommercialOrganization("BlackBox");
// Add services to the container.
builder.Services.AddMemoryCache();
builder.Services.AddControllers();
//-------------------Registration of services------------------------//
builder.Services.AddSingleton<IAccountService, AccountService>();
builder.Services.AddSingleton<IDashboardService, DashboardService>();
builder.Services.AddSingleton<IReportsService, ReportsService>();
builder.Services.AddSingleton<IVehicleStatusService, VehicleStatusService>();



//-------------------Registration of repositories------------------------//
builder.Services.AddSingleton<IAccountRepository, AccountRepository>();
builder.Services.AddSingleton<IDashboardRepository, DashboardRepository>();
builder.Services.AddSingleton<IReportsRepository, ReportsRepository>();
builder.Services.AddSingleton<IVehicleStatusRepository, VehicleStatusRepository>();

//-------------------Registration of repositories------------------------//
builder.Services.AddSingleton<ImportExportExcelService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
