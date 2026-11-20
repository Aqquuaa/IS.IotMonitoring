using IS.IotMonitoring.DataGenerator.Api;
using IS.IotMonitoring.DataGenerator.Options;
using IS.IotMonitoring.DataGenerator.Simulation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

builder.Services.AddOptions<GeneratorOptions>()
    .Bind(builder.Configuration.GetSection(GeneratorOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddHttpClient(IotApiClient.HttpClientName, client => client.Timeout = TimeSpan.FromSeconds(10));
builder.Services.AddSingleton<IotApiClient>();
builder.Services.AddHostedService<TelemetrySimulator>();

await builder.Build().RunAsync();
