using Trackmaster_Repository.Interface;
using Trackmaster_Repository.Repository;
using Trackmaster_Service.Interface;
using Trackmaster_Service.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
//-------------------Registration of services------------------------//
builder.Services.AddSingleton<IAccountService, AccountService>();


//-------------------Registration of repositories------------------------//
builder.Services.AddSingleton<IAccountRepository, AccountRepository>();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
