using IS.IotMonitoring.CoreApi.Application.Repositories;
using IS.IotMonitoring.CoreApi.Application.Services;
using IS.IotMonitoring.CoreApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

try
{
    BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
}
catch (BsonSerializationException)
{
    // Ignore if already registered
}

// 1. PostgreSQL & Device Repository
builder.Services.AddDbContext<DeviceDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));
builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();
// 2. MongoDB & Telemetry Repository
builder.Services.AddSingleton<IMongoClient>(_ =>
    new MongoClient(builder.Configuration.GetConnectionString("MongoDb")));
builder.Services.AddScoped<IMongoDatabase>(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase(builder.Configuration["MongoDb:DatabaseName"] ?? "iot_monitoring"));
builder.Services.AddScoped<ITelemetryRepository, TelemetryRepository>();
// 3. Application Services
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<ITelemetryService, TelemetryService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DeviceDbContext>();
    try
    {
        await db.Database.EnsureCreatedAsync();
    }
    catch (Exception ex)
    {
        // Put a breakpoint here, or look at your console to see the REAL error
        Console.WriteLine($"THE REAL ERROR IS: {ex.InnerException?.Message ?? ex.Message}");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();


app.UseAuthorization();

app.MapControllers();

app.Run();
