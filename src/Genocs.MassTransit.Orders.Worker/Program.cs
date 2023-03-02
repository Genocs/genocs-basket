using Azure.Monitor.OpenTelemetry.Exporter;
using Genocs.MassTransit.Inventories.Contracts;
using Genocs.MassTransit.Orders.Components.Consumers;
using Genocs.MassTransit.Orders.Components.CourierActivities;
using Genocs.MassTransit.Orders.Components.HttpClients;
using Genocs.MassTransit.Orders.Components.StateMachines;
using Genocs.MassTransit.Orders.Components.StateMachines.Activities;
using Genocs.MassTransit.Orders.Worker;
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

        // This is a state machine Activity
        services.AddScoped<OrderRequestedActivity>();

        //services.AddScoped<RoutingSlipBatchEventConsumer>();

        services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);
        services.AddMassTransit(cfg =>
        {
            // Consumer configuration
            cfg.AddConsumersFromNamespaceContaining<FulfillOrderConsumer>();

            // Routing slip configuration
            cfg.AddActivitiesFromNamespaceContaining<AllocateInventoryActivity>();

            // Saga handling Order state
            cfg.AddSagaStateMachine<OrderStateMachine, OrderState>(typeof(OrderStateMachineDefinition))
                .RedisRepository(); // Redis as Saga persistence

            // Saga handling Payment state
            cfg.AddSagaStateMachine<PaymentStateStateMachine, PaymentState>(typeof(PaymentStateStateMachineDefinition))
                .RedisRepository();

            cfg.UsingRabbitMq(ConfigureBus);

            // Request client configuration
            //{ KebabCaseEndpointNameFormatter.Instance.Consumer<AllocateInventory>()}
            cfg.AddRequestClient<AllocateInventory>(new Uri($"exchange:Genocs.MassTransit.Inventories.Contracts:AllocateInventory"));

        });

        services.AddHostedService<MassTransitConsoleHostedService>();

        // Add services to the container.
        services.AddHttpClient<CustomerClient>();

        // Set Custom Open telemetry
        services.AddOpenTelemetryTracing(builder =>
        {
            // Remove comment below to enable tracing on console 
            //builder.AddConsoleExporter();
            TracerProviderBuilder provider = builder.SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService("OrdersWorker")
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
    configurator.UseMessageScheduler(new Uri("queue:quartz"));

    // This configuration will configure the Activity Definition
    configurator.ConfigureEndpoints(context);
}