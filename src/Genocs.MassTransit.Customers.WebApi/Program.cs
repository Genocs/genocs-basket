using Azure.Monitor.OpenTelemetry.Exporter;
using Genocs.MassTransit.Customers.WebApi;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("MassTransit", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();


var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) =>
{
    lc.WriteTo.Console();

    // Check for Azure ApplicationInsights 
    string? applicationInsightsConnectionString = ctx.Configuration.GetConnectionString(Constants.ApplicationInsightsConnectionString);
    if (!string.IsNullOrWhiteSpace(applicationInsightsConnectionString))
    {
        lc.WriteTo.ApplicationInsights(new TelemetryConfiguration
        {
            ConnectionString = applicationInsightsConnectionString
        }, TelemetryConverter.Traces);
    }
});

// add services to DI container
var services = builder.Services;

// ***********************************************
// Azure Application Insight configuration - START
//services.AddCustomOpenTelemetry(builder.Configuration);

//services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((module, o) =>
//{
//    module.IncludeDiagnosticSourceActivities.Add("MassTransit");
//    TelemetryConfiguration configuration = TelemetryConfiguration.CreateDefault();
//    configuration.ConnectionString = builder.Configuration.GetConnectionString("ApplicationInsights");
//    configuration.TelemetryInitializers.Add(new HttpDependenciesParsingTelemetryInitializer());
//    _telemetryClient = new TelemetryClient(configuration);
//});
// Azure Application Insight configuration - END
// ***********************************************



services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services.Configure<HealthCheckPublisherOptions>(options =>
{
    options.Delay = TimeSpan.FromSeconds(2);
    options.Predicate = check => check.Tags.Contains("ready");

});

//builder.Services.Configure<JsonOptions>(options =>
//{
//    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
//    options.JsonSerializerOptions.PropertyNamingPolicy = null;
//});
string? applicationInsightsConnectionString = builder.Configuration.GetConnectionString(Constants.ApplicationInsightsConnectionString);


// Set Custom Open telemetry
services.AddOpenTelemetryTracing(builder =>
{
    builder.AddAspNetCoreInstrumentation();


    TracerProviderBuilder provider = builder.SetResourceBuilder(ResourceBuilder.CreateDefault()
            .AddService("CustomersWebApi")
            .AddTelemetrySdk()
            .AddEnvironmentVariableDetector())
        .AddSource("*");
    //.AddMongoDBInstrumentation()
    provider.AddAzureMonitorTraceExporter(o =>
        {
            o.ConnectionString = applicationInsightsConnectionString;
        });

    provider.AddJaegerExporter(o =>
        {
            o.AgentHost = "localhost";
            o.AgentPort = 6831;
            o.MaxPayloadSizeInBytes = 4096;
            o.ExportProcessorType = ExportProcessorType.Batch;
            o.BatchExportProcessorOptions = new BatchExportProcessorOptions<System.Diagnostics.Activity>
            {
                MaxQueueSize = 2048,
                ScheduledDelayMilliseconds = 5000,
                ExporterTimeoutMilliseconds = 30000,
                MaxExportBatchSize = 512,
            };
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();

//await TelemetryAndLogging.FlushAndCloseAsync();

Log.CloseAndFlush();
