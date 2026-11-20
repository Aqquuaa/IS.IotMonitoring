using IS.IotMonitoring.AuthService.Options;
using IS.IotMonitoring.AuthService.Persistence;
using IS.IotMonitoring.AuthService.Security;
using IS.IotMonitoring.AuthService.Services;
using Microsoft.EntityFrameworkCore;
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
        labels: [new LokiLabel { Key = "service", Value = "authapi" }]));
//SERILOG

// TEMPO TRACING
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("authapi"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation(options =>
            options.Filter = context => !context.Request.Path.StartsWithSegments("/metrics"))
        .AddNpgsql()
        .AddOtlpExporter(options => options.Endpoint = new Uri(builder.Configuration["Tempo:Url"] ?? "http://localhost:4317")));
// TEMPO TRACING

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.Configure<AdminSeedOptions>(builder.Configuration.GetSection(AdminSeedOptions.SectionName));

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

builder.Services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IAccountService, AccountService>();

var app = builder.Build();

app.UseSerilogRequestLogging();

// PROMETHEUS METRICS
app.UseHttpMetrics();
app.MapMetrics();
// PROMETHEUS METRICS

await app.Services.InitializeDatabaseAsync();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}



app.UseHttpsRedirection();

app.MapControllers();

app.Run();
