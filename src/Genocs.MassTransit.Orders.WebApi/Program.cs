using Azure.Monitor.OpenTelemetry.Exporter;
using Genocs.MassTransit.Orders.Contracts;
using Genocs.MassTransit.Orders.WebApi;
using MassTransit;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

string? applicationInsightsConnectionString = builder.Configuration.GetConnectionString(Constants.ApplicationInsightsConnectionString);

builder.Host.UseSerilog((ctx, lc) =>
{
    lc.WriteTo.Console();

    // Check for Azure ApplicationInsights 
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

services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);
services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        //cfg.Host();

        MessageDataDefaults.ExtraTimeToLive = TimeSpan.FromDays(1);
        MessageDataDefaults.Threshold = 2000;
        MessageDataDefaults.AlwaysWriteToRepository = false;

        //cfg.UseMessageData(new MongoDbMessageDataRepository(IsRunningInContainer ? "mongodb://mongo" : "mongodb://127.0.0.1", "attachments"));
    });

    //mt.AddRequestClient<SubmitOrder>(new Uri($"queue:{KebabCaseEndpointNameFormatter.Instance.Consumer<SubmitOrderConsumer>()}"));

    x.AddRequestClient<OrderStatus>();
    x.AddRequestClient<PaymentStatus>();
    x.AddRequestClient<PaymentRequest>();

});


// Set Custom Open telemetry
services.AddOpenTelemetry().WithTracing(builder =>
{
    TracerProviderBuilder provider = builder.SetResourceBuilder(ResourceBuilder.CreateDefault()
            .AddService("OrdersWebApi")
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

Log.CloseAndFlush();
