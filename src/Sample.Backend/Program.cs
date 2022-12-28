using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using MassTransit;
using MassTransit.Configuration;
using Sample.Components.StateMachines;
using Sample.Contracts;
using Sample.Shared;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("MassTransit", LogEventLevel.Debug)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var host = Host.CreateDefaultBuilder(args)
    .UseSerilog()
    .ConfigureServices((hostContext, services) =>
    {
        services.AddMassTransit(x =>
        {
            // we aren't using the bus, only Kafka
            x.UsingInMemory();

            x.AddRider(r =>
            {
                r.AddKafkaComponents();

                r.AddSagaStateMachine<ContainerStateMachine, ContainerState>()
                    .MongoDbRepository(hostContext.Configuration.GetConnectionString("MongoDb"), cfg => { cfg.DatabaseName = "KafkaDemo"; });

                r.UsingKafka((context, cfg) =>
                {
                    cfg.Host(context);

                    const string topicName = @"^events\.warehouse\.[0-9]*";

                    cfg.TopicEndpoint<string, WarehouseEvent>(topicName, "sample.backend", e =>
                    {
                        e.AutoOffsetReset = AutoOffsetReset.Earliest;

                        e.DiscardSkippedMessages();

                        e.SetValueDeserializer(new AvroDeserializer<WarehouseEvent>(context.GetService<ISchemaRegistryClient>()).AsSyncOverAsync());

                        e.AddPrePipeSpecification(new FilterPipeSpecification<ConsumeContext>(new AvroFilter<WarehouseEvent>(m => m.Event)));

                        e.UseKillSwitch(k => k.SetActivationThreshold(1).SetRestartTimeout(m: 1).SetTripThreshold(0.2).SetTrackingPeriod(m: 1));
                        e.UseMessageRetry(retry => retry.Interval(1000, TimeSpan.FromSeconds(1)));

                        e.ConfigureSagas(context);
                    });
                });
            });
        });
    })
    .Build();

await host.RunAsync();