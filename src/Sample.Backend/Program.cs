using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Sample.Components;
using Sample.Components.Consumers;
using Sample.Components.StateMachines;
using Sample.Contracts;
using Sample.Shared;
using Serilog;
using Serilog.Events;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureSerilog()
    .ConfigureServices((hostContext, services) =>
    {
        services.AddMassTransit(x =>
        {
            // we aren't using the bus, only Kafka
            x.UsingInMemory();

            x.AddRider(r =>
            {
                r.AddKafkaComponents();

                x.AddMongoDbConfiguration(hostContext.Configuration);

                r.AddConsumer<ShipmentManifestReceivedConsumer>();

                r.AddSagaStateMachine<ContainerStateMachine, ContainerState>()
                    .MongoDbRepository(hostContext.Configuration.GetConnectionString("MongoDb"), cfg =>
                    {
                        cfg.DatabaseName = "KafkaDemo";
                    });

                r.UsingKafka((context, cfg) =>
                {
                    cfg.Host(context);

                    cfg.ClientId = "BackEnd";

                    cfg.TopicEndpoint<string, ShipmentManifestReceived>("events.erp", "erp.backend", e =>
                    {
                        e.AutoOffsetReset = AutoOffsetReset.Earliest;

                        e.SetValueDeserializer(new AvroDeserializer<ShipmentManifestReceived>(context.GetRequiredService<ISchemaRegistryClient>())
                            .AsSyncOverAsync());

                        e.ConcurrentMessageLimit = 10;

                        e.UseSampleRetryConfiguration();

                        e.ConfigureConsumer<ShipmentManifestReceivedConsumer>(context);
                    });

                    const string topicName = @"^events\.warehouse\.[0-9]*";

                    cfg.TopicEndpoint<string, WarehouseEvent>(topicName, "warehouse.backend", e =>
                    {
                        e.AutoOffsetReset = AutoOffsetReset.Earliest;

                        e.SetValueDeserializer(new AvroDeserializer<WarehouseEvent>(context.GetRequiredService<ISchemaRegistryClient>()).AsSyncOverAsync());

                        // the number of concurrent messages, per partition
                        e.ConcurrentMessageLimit = 10;

                        // create up to two Confluent Kafka consumers, increases throughput with multiple partitions
                        e.ConcurrentConsumerLimit = 2;

                        // delivery only one message per key value within a partition at a time (default)
                        e.ConcurrentDeliveryLimit = 1;

                        // Adding this filter allows AVRO union messages to be consumed directly
                        e.UseAvroUnionMessageTypeFilter<WarehouseEvent>(m => m.Event);

                        e.UseSampleRetryConfiguration();

                        // ignore these messages, move past them, not used, but avoid errors it's one of the AVRO union types
                        // because DiscardSkippedMessages doesn't seem to work properly with topic endpoint
                        e.Handler<ProductReceived>(_ => Task.CompletedTask);

                        e.ConfigureSaga<ContainerState>(context);
                    });
                });
            });
        });
    })
    .Build();

await host.RunAsync();