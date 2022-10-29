using Genocs.MassTransit.Inventories.Components.Consumers;
using Genocs.MassTransit.Inventories.Components.StateMachines;
using Genocs.MassTransit.Inventories.Worker;
using MassTransit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog;
using Serilog.Events;


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

Microsoft.Extensions.Hosting.IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        var connectionString = hostContext.Configuration.GetConnectionString(Constants.ApplicationInsightsConnectionString);
        TelemetryAndLogging.Initialize(connectionString);

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