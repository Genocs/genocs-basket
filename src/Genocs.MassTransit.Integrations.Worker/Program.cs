using Azure.Monitor.OpenTelemetry.Exporter;
using Genocs.MassTransit.Integrations.Worker;
using Genocs.MassTransit.Integrations.Worker.Options;
using MassTransit.Definition;
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
        // Read Settings
        string? applicationInsightsConnectionString = hostContext.Configuration.GetConnectionString(Constants.ApplicationInsightsConnectionString);
        string serviceName = hostContext.Configuration.GetValue(typeof(string), Constants.ServiceName) as string ?? "IntegrationsWorker";

        MonitoringSettings settings = new MonitoringSettings();
        hostContext.Configuration.Bind(MonitoringSettings.Position, settings);
        services.AddSingleton(settings);

        services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);

        services.AddHostedService<ConsoleHostedService>();

        // Set Custom Open telemetry
        services.AddOpenTelemetry().WithTracing(builder =>
        {
            TracerProviderBuilder provider = builder.SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService(serviceName)
                    .AddTelemetrySdk()
                    .AddEnvironmentVariableDetector())
                .AddSource("*");

            // Remove comment below to enable tracing on console
            // you should add MongoDB.Driver.Core.Extensions.OpenTelemetry nuget package
            provider.AddMongoDBInstrumentation();

            // Remove comment below to enable tracing on console
            // you should add OpenTelemetry.Exporter.Console nuget package
            provider.AddConsoleExporter();


            // Check for Azure ApplicationInsights 
            if (!string.IsNullOrWhiteSpace(applicationInsightsConnectionString))
            {
                provider.AddAzureMonitorTraceExporter(o =>
                    {
                        o.ConnectionString = applicationInsightsConnectionString;
                    });
            }

            provider.AddJaegerExporter(o =>
                {
                    o.AgentHost = settings.Jaeger;
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

Log.CloseAndFlush();
