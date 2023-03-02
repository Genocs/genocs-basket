using Azure.Monitor.OpenTelemetry.Exporter;
using Genocs.MassTransit.Integrations.Worker;
using MassTransit;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

IHost host = Host.CreateDefaultBuilder(args)
    .UseSerilog((ctx, lc) =>
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
    })
    .ConfigureServices((hostContext, services) =>
    {
        string? applicationInsightsConnectionString = hostContext.Configuration.GetConnectionString(Constants.ApplicationInsightsConnectionString);
        //TelemetryAndLogging.Initialize(connectionString);

        services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);

        services.AddHostedService<ConsoleHostedService>();

        // Set Custom Open telemetry
        services.AddOpenTelemetryTracing(builder =>
        {
            TracerProviderBuilder provider = builder.SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService("IntegrationsWorker")
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
    })
    .ConfigureLogging((hostingContext, logging) =>
    {
        logging.AddSerilog(dispose: true);
        logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
    })
    .Build();

await host.RunAsync();

await TelemetryAndLogging.FlushAndCloseAsync();

Log.CloseAndFlush();
