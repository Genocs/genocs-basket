﻿using Azure.Monitor.OpenTelemetry.Exporter;
using Genocs.MassTransit.Inventories.Components.Consumers;
using Genocs.MassTransit.Inventories.Components.StateMachines;
using Genocs.MassTransit.Inventories.Worker;
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
        services.AddMassTransit(cfg =>
        {
            // Consumer configuration
            cfg.AddConsumersFromNamespaceContaining<AllocateInventoryConsumer>();

            cfg.AddPublishMessageScheduler();

            // Used to handle delayed messages using rabbitMQ capability
            cfg.AddDelayedMessageScheduler();

            // Routing slip configuration

            // Saga handling Allocation Inventory state
            cfg.AddSagaStateMachine<AllocationStateMachine, AllocationState>(typeof(AllocationStateMachineDefinition))
                //.RedisRepository(); // Redis as Saga persistence
                .MongoDbRepository(r =>
                {
                    r.Connection = hostContext.Configuration.GetConnectionString(Constants.MongoDbConnectionString);
                    r.DatabaseName = "allocation";
                });

            cfg.UsingRabbitMq(ConfigureBus);
        });

        services.AddHostedService<MassTransitConsoleHostedService>();

        // Set Custom Open telemetry
        services.AddOpenTelemetry().WithTracing(builder =>
        {
            TracerProviderBuilder provider = builder.SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService("InventoriesWorker")
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

Log.CloseAndFlush();


static void ConfigureBus(IBusRegistrationContext context, IRabbitMqBusFactoryConfigurator configurator)
{
    //configurator.UseMessageData(new MongoDbMessageDataRepository("mongodb://127.0.0.1", "attachments"));

    //configurator.ReceiveEndpoint(KebabCaseEndpointNameFormatter.Instance.Consumer<RoutingSlipBatchEventConsumer>(), e =>
    //{
    //    e.PrefetchCount = 20;

    //    e.Batch<RoutingSlipCompleted>(b =>
    //    {
    //        b.MessageLimit = 10;
    //        b.TimeLimit = TimeSpan.FromSeconds(5);

    //        b.Consumer<RoutingSlipBatchEventConsumer, RoutingSlipCompleted>(context);
    //    });
    //});

    // This configuration allow to handle the Scheduling
    //configurator.UseMessageScheduler(new Uri("queue:quartz"));

    configurator.UseDelayedMessageScheduler();


    // This configuration will configure the Activity Definition
    configurator.ConfigureEndpoints(context);
}