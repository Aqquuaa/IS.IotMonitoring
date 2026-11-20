using IS.IotMonitoring.CoreApi.Application.Metrics;
using IS.IotMonitoring.CoreApi.Application.Repositories;
using IS.IotMonitoring.CoreApi.Application.Services;
using IS.IotMonitoring.CoreApi.Infrastructure.Persistence;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Npgsql;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Grafana.Loki;

var builder = WebApplication.CreateBuilder(args);

//SERILOG
builder.Services.AddSerilog(logger => logger
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.GrafanaLoki(builder.Configuration["Loki:Url"] ?? "http://localhost:3100",
        labels: [new LokiLabel { Key = "service", Value = "coreapi" }]));
//SERILOG

// TEMPO TRACING
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("coreapi"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation(options =>
            options.Filter = context => !context.Request.Path.StartsWithSegments("/metrics"))
        .AddNpgsql()
        .AddOtlpExporter(options => options.Endpoint = new Uri(builder.Configuration["Tempo:Url"] ?? "http://localhost:4317")));
// TEMPO TRACING

// JWT AUTHENTICATION (tokens issued by AuthService)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SigningKey"]
                    ?? throw new InvalidOperationException("Jwt:SigningKey is not configured."))),
            NameClaimType = "name",
            RoleClaimType = "role"
        };
    });
builder.Services.AddAuthorization();
// JWT AUTHENTICATION

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

try{ BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard)); } catch (BsonSerializationException){ }

//POSTGRES / DEVICE REPOSITORY
builder.Services.AddDbContext<DeviceDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));
builder.Services.AddSingleton<TelemetryMetrics>();
builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();
//POSTGRES / DEVICE REPOSITORY

//MONGODB / TELEMETRY REPOSITORY
builder.Services.AddSingleton<IMongoClient>(_ =>
    new MongoClient(builder.Configuration.GetConnectionString("MongoDb")));
builder.Services.AddScoped<IMongoDatabase>(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase(builder.Configuration["MongoDb:DatabaseName"] ?? "iot_monitoring"));
builder.Services.AddScoped<ITelemetryRepository, TelemetryRepository>();
//MONGODB / TELEMETRY REPOSITORY

// 3. Application Services
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<ITelemetryService, TelemetryService>();

var app = builder.Build();

app.UseSerilogRequestLogging();

// PROMETHEUS METRICS
app.UseHttpMetrics();
app.MapMetrics();
// PROMETHEUS METRICS

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DeviceDbContext>();
    try
    {
        await db.Database.EnsureCreatedAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR IS: {ex.InnerException?.Message ?? ex.Message}");
        throw;
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
